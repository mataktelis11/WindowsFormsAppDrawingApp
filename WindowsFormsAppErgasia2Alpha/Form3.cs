using System;
using System.Data;
using System.Data.OleDb;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace WindowsFormsAppErgasia2Alpha
{
    public partial class Form3 : Form
    {
        //this forms has a datagrindview
        //candisplay and delete projects
        Form1 form1;
        String connectionString;
        OleDbConnection connection;

        public Form3(Form1 form1)
        {
            InitializeComponent();
            this.form1 = form1;
        }

        private void Form3_Load(object sender, EventArgs e)
        {
            connectionString = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=DatabaseDrawingApp.mdb";
            connection = new OleDbConnection(connectionString);
            loadDB();

            dataGridView1.Location = new Point((this.Width) / 2 - dataGridView1.Width / 2, this.Height / 2 - dataGridView1.Height / 2 - 25);//set the location of the dgv
        }

        //load DB to datagrindview method
        public void loadDB()
        {
            connection.Open();
            String query = "Select Nickname,TimeStamp From Drawings";
            OleDbCommand command = new OleDbCommand(query, connection);

            OleDbDataAdapter adapter = new OleDbDataAdapter(command);
            DataTable dataTable = new DataTable();

            adapter.Fill(dataTable);
            dataGridView1.DataSource = dataTable;
            connection.Close();

            dataGridView1.Columns[0].HeaderText = "Project Name";
            dataGridView1.Columns[1].HeaderText = " Project Timestamp";
        }


        //button: display project
        private void button1_Click_1(object sender, EventArgs e)
        {
            //the user has to select only one row
            if (dataGridView1.SelectedRows.Count == 1)
            {
                //MessageBox.Show(dataGridView1.SelectedRows[0].Cells[0].Value.ToString());
                string nickname = dataGridView1.SelectedRows[0].Cells[0].Value.ToString();

                connection.Open();
                String query = "Select * From Drawings where Nickname=@nickname";
                OleDbCommand command = new OleDbCommand(query, connection);
                command.Parameters.AddWithValue("@nickname", nickname);
                OleDbDataReader reader = command.ExecuteReader();
                StringBuilder builder = new StringBuilder();
                while (reader.Read())
                {
                    //Console.WriteLine(reader.GetString(3));
                    form1.parser(reader.GetString(3));//display
                    
                    //MessageBox.Show(reader.GetString(3));
                    form1.Focus();  //focus on form1
                }
                connection.Close();

            }
            else
            {
                MessageBox.Show("Please select one row from the table", "My Drawing Application", 0, MessageBoxIcon.Exclamation);
            }
        }

        //method delete
        private void delete(string name) 
        {
            connection.Open();
            String query = "DELETE FROM Drawings WHERE Nickname=@a;";
            OleDbCommand command = new OleDbCommand(query, connection);
            command.Parameters.AddWithValue("@a", name);
            int count = command.ExecuteNonQuery();
            connection.Close();
        }

        //button delete project
        private void button2_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count == 1)
            {
                if (MessageBox.Show("Are you sure you want to delete project '" + dataGridView1.SelectedRows[0].Cells[0].Value.ToString() + "' ?", "My Drawing Application", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    delete(dataGridView1.SelectedRows[0].Cells[0].Value.ToString());
                    loadDB();
                }
            }
            else
            {
                MessageBox.Show("Please select one row from the table", "My Drawing Application", 0, MessageBoxIcon.Exclamation);
            }



        }

        private void Form3_FormClosed(object sender, FormClosedEventArgs e)
        {
            form1.form3Active = false;//similar to form2
        }

        //contextmenustrip for datagrindview
        private void displaySelectedProjectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            button1.PerformClick();
        }

        private void deleteSelectedProjectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            button2.PerformClick();
        }

        private void Form3_Resize(object sender, EventArgs e)
        {
            dataGridView1.Location = new Point((this.Width) / 2 - dataGridView1.Width / 2, this.Height / 2 - dataGridView1.Height / 2 - 25);
        }
    }
}
