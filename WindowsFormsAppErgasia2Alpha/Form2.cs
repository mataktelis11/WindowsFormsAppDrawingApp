using System;
using System.Data.OleDb;
using System.Windows.Forms;

namespace WindowsFormsAppErgasia2Alpha
{
    public partial class Form2 : Form
    {
        //this for just uploads the project
        String connectionString;
        OleDbConnection connection;
        
        Form1 form1;
        public Form2(Form1 form1)
        {
            InitializeComponent();         
            this.form1 = form1; //have form1 for reference
        }

        //button upload to database
        private void button1_Click(object sender, EventArgs e)
        {
            if(textBox1.Text.Length == 0)
            {
                MessageBox.Show("Please give a name to the project.", "My drawing application", 0, MessageBoxIcon.Error);
                return;
            }
            else 
            {
                //first check if there is another project with the same name
                connection.Open();
                String query = "Select Nickname From Drawings";
                OleDbCommand command = new OleDbCommand(query, connection);
                OleDbDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    if (reader.GetString(0).Equals(textBox1.Text))
                    {
                        MessageBox.Show("A project with this name already exists", "My Drawing Application", 0, MessageBoxIcon.Error);
                        connection.Close();
                        return;
                    }
                }
                connection.Close();


                //else upload
                connection.Open();
                String query2 = "Insert into Drawings (Nickname, [TimeStamp],[Shapes]) values (@a,@b,@c);";
                OleDbCommand command2 = new OleDbCommand(query2, connection);

                command2.Parameters.AddWithValue("@a", textBox1.Text);
                command2.Parameters.AddWithValue("@b", DateTime.Now.ToString());
                command2.Parameters.AddWithValue("@c", form1.shapesDrawnToData());
                int count = command2.ExecuteNonQuery();
                connection.Close();
                MessageBox.Show("Project uploaded to the database successfully!", "My Drawing Application", 0, MessageBoxIcon.Information);
                

                if(form1.form3Active)
                    form1.form3.loadDB();//if form3 is open , update its datagrindview
                this.Close();

            }
        }

        private void Form2_Load(object sender, EventArgs e)
        {
            connectionString = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=DatabaseDrawingApp.mdb";
            connection = new OleDbConnection(connectionString);
        }

        private void Form2_FormClosed(object sender, FormClosedEventArgs e)
        {
            form1.form2Active = false;//indictator to form1 that form2 obj has closed
        }
    }
}
