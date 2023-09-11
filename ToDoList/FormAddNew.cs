using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ToDoList
{
    public partial class FormAddNew : Form
    {
        private int lastID;
        public FormAddNew()
        {
            InitializeComponent();
        }



        private void FormAddNew_Load(object sender, EventArgs e)
        {
            this.AcceptButton = buttonAdd;
        }


        // button to close the form
        private void buttonClose_Click(object sender, EventArgs e)
        {
            ToDoForm.connection.Close();
            Close();
        }


        // add a new task
        private void buttonAdd_Click(object sender, EventArgs e)
        {
            ToDoForm.connection.Open();
            string textTask = textBoxTask.Text;

            string insertQuery = "INSERT INTO todotable (IsCheck, Text, Complete, Date, Datedone) VALUES (@IsCheck, @Text, @Complete, @Date, @Datedone)";
            using (SqlCommand command = new SqlCommand(insertQuery, ToDoForm.connection))
            {
                command.Parameters.AddWithValue("@IsCheck", false);
                command.Parameters.AddWithValue("@Text", textTask);
                command.Parameters.AddWithValue("@Complete", false);
                command.Parameters.AddWithValue("@Date", DateTime.Now.ToString("yyyy-MM-dd"));
                command.Parameters.AddWithValue("@Datedone", dateTimePicker1.Value.ToString("yyyy-MM-dd"));


                command.ExecuteNonQuery();
            }

            string selectQuery = "SELECT MAX(ID) AS LastID FROM todotable";

            using (SqlCommand command = new SqlCommand(selectQuery, ToDoForm.connection))
            {
                object result = command.ExecuteScalar();

                if (result != null && result != DBNull.Value)
                {
                    lastID = Convert.ToInt32(result);
                }
            }

            DataRow newRow = ToDoForm.dataTable.NewRow();
            newRow["ID"] = lastID;
            newRow["IsCheck"] = false;
            newRow["Text"] = textTask;
            newRow["Complete"] = false;
            newRow["Date"] = DateTime.Now.ToString("yyyy-MM-dd");
            newRow["Datedone"] = dateTimePicker1.Value.ToString("yyyy-MM-dd");

            ToDoForm.dataTable.Rows.Add(newRow);
            ToDoForm.connection.Close();
            Close();
        }
    }
}
