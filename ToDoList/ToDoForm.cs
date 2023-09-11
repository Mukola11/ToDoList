using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ToDoList
{


    public partial class ToDoForm : Form
    {
        public static SqlConnection connection;
        private SqlDataAdapter dataAdapter;
        public static DataTable dataTable;
        private Point startPoint;
        private bool isDragging = false;
        private bool isFullscreen = false;
        private List<DataRow> changedRows = new List<DataRow>();
        private List<DataRow> rowsToRemove = new List<DataRow>();


        public ToDoForm()
        {
            InitializeComponent();

            // connection to the database
            string connectionString = "Server = admin; Database = ToDodata; Trusted_Connection = True;";
            connection = new SqlConnection(connectionString);

            string query = "SELECT * FROM todotable";
            dataAdapter = new SqlDataAdapter(query, connection);
            SqlCommandBuilder builder = new SqlCommandBuilder(dataAdapter);

            dataTable = new DataTable();
            dataAdapter.Fill(dataTable);

            dataGridView1.DataSource = dataTable;
            dataGridView1.CellValueChanged += dataGridView1_CellValueChanged;
            InitializeEvent();
        }


        private void Form1_Load(object sender, EventArgs e)
        {
            // TODO: This line of code loads data into the 'toDodataDataSet.todotable' table. You can move, or remove it, as needed.
            this.todotableTableAdapter.Fill(this.toDodataDataSet.todotable);
        }


        // ability to move the application by moving the panel
        private void controlPanel_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDragging)
            {
                Point endPoint = PointToScreen(new Point(e.X, e.Y));
                Location = new Point(endPoint.X - startPoint.X, endPoint.Y - startPoint.Y);
            }
        }
        private void controlPanel_MouseDown(object sender, MouseEventArgs e)
        {
            isDragging = true;
            startPoint = new Point(e.X, e.Y);
        }

        private void controlPanel_MouseUp(object sender, MouseEventArgs e)
        {
            isDragging = false;
        }


        // button to close the application
        private void pictureBoxClose_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }


        // button for the application size
        private void pictureBoxMax_Click(object sender, EventArgs e)
        {
            if (isFullscreen)
            {
                WindowState = FormWindowState.Normal;
                isFullscreen = false;
            }
            else
            {
                WindowState = FormWindowState.Maximized;
                isFullscreen = true;
            }
        }


        // button to minimise the application
        private void pictureBoxMin_Click(object sender, EventArgs e)
        {
            WindowState = FormWindowState.Minimized;
        }


        //button for the ability to delete rows
        private void buttonEdit_Click(object sender, EventArgs e)
        {
            dataGridView1.Columns["isCheckDataGridViewCheckBoxColumn"].Visible = !dataGridView1.Columns["isCheckDataGridViewCheckBoxColumn"].Visible;

            if (dataGridView1.Columns["isCheckDataGridViewCheckBoxColumn"].Visible == false)
            {
                buttonRemove.Visible = false;
                dataGridView1.Columns["completeDataGridViewCheckBoxColumn"].ReadOnly = false;
                buttonAdd.Enabled = true;
            }
        }


        // button to open a new window
        private void buttonAdd_Click(object sender, EventArgs e)
        {
            FormAddNew newForm = new FormAddNew();
            newForm.ShowDialog();
        }



        private void dataGridView1_CurrentCellDirtyStateChanged(object sender, EventArgs e)
        {
            if (dataGridView1.IsCurrentCellDirty)
            {
                dataGridView1.CommitEdit(DataGridViewDataErrorContexts.Commit);
                dataGridView1.EndEdit();
            }
        }


        // function for tracking changes in cell properties
        private void dataGridView1_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex >= 0 && e.ColumnIndex == 5)
            {
                buttonSave.Visible = true;
                buttonRemove.Visible = false;
                buttonEdit.Enabled = false;
                buttonAdd.Enabled = false;

                DataRow changedRow = dataTable.Rows[e.RowIndex];
                changedRows.Add(changedRow);
            }
            if (e.RowIndex >= 0 && e.ColumnIndex >= 0 && e.ColumnIndex == 1)
            {
                buttonRemove.Visible = true;
                buttonSave.Visible = false;
                dataGridView1.Columns["completeDataGridViewCheckBoxColumn"].ReadOnly = true;
                buttonAdd.Enabled = false;

                DataRow removedRow = dataTable.Rows[e.RowIndex];
                if (!rowsToRemove.Contains(removedRow))
                {
                    rowsToRemove.Add(removedRow);
                }

            }
        }


        // button for save data
        private void buttonSave_Click(object sender, EventArgs e)
        {
            connection.Open();

            string updateQuery = "UPDATE todotable SET Complete = @Complete WHERE ID = @ID";
            foreach (DataRow changedRow in changedRows)
            {
                using (SqlCommand command = new SqlCommand(updateQuery, connection))
                {
                    command.Parameters.AddWithValue("Complete", changedRow["Complete"]);
                    command.Parameters.AddWithValue("@ID", changedRow["ID"]);

                    command.ExecuteNonQuery();
                }
            }
            connection.Close();
            changedRows.Clear();
            buttonSave.Visible = false;
            buttonEdit.Enabled = true;
            buttonAdd.Enabled = true;
        }


        // button for remove data
        private void buttonRemove_Click(object sender, EventArgs e)
        {
            connection.Open();
            string removeQuery = "DELETE FROM todotable WHERE ID = @ID";
            foreach (DataRow rowToRemove in rowsToRemove)
            {
                using (SqlCommand command = new SqlCommand(removeQuery, connection))
                {
                    command.Parameters.AddWithValue("@ID", rowToRemove["ID"]);
                    command.ExecuteNonQuery();
                }
                dataTable.Rows.Remove(rowToRemove);


            }
            connection.Close();

            rowsToRemove.Clear();
            buttonRemove.Visible = false;
            dataGridView1.Columns["isCheckDataGridViewCheckBoxColumn"].Visible = false;
            dataGridView1.Columns["completeDataGridViewCheckBoxColumn"].ReadOnly = false;
            buttonAdd.Enabled = true;
        }


        private void ApplyFilter(string operation)
        {
            if (operation == "filterComplete")
            {
                dataTable.DefaultView.RowFilter = "Complete = true";
            }

            if (operation == "filterPending")
            {
                dataTable.DefaultView.RowFilter = "Complete = false";
            }
            if (operation == "filterAll")
            {
                dataTable.DefaultView.RowFilter = "";
            }
        }

        private void labelAll_Click(object sender, EventArgs e)
        {
            ApplyFilter("filterAll");
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            ApplyFilter("filterAll");
        }


        private void labelComplete_Click(object sender, EventArgs e)
        {
            ApplyFilter("filterComplete");
        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {
            ApplyFilter("filterComplete");
        }


        private void labelPending_Click(object sender, EventArgs e)
        {
            ApplyFilter("filterPending");
        }

        private void pictureBox3_Click(object sender, EventArgs e)
        {
            ApplyFilter("filterPending");
        }


        // change the colour of the grid
        private void dataGridView1_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex >= 0)
            {
                if (e.RowIndex % 2 == 0)
                {
                    e.CellStyle.BackColor = Color.FromArgb(240, 225, 185);
                    dataGridView1.Rows[e.RowIndex].DefaultCellStyle.SelectionBackColor = Color.FromArgb(255, 228, 181);
                    dataGridView1.Rows[e.RowIndex].DefaultCellStyle.SelectionForeColor = Color.Black;
                }
                else
                {
                    e.CellStyle.BackColor = Color.FromArgb(255, 240, 200);
                    dataGridView1.Rows[e.RowIndex].DefaultCellStyle.SelectionBackColor = Color.FromArgb(255, 228, 181);
                    dataGridView1.Rows[e.RowIndex].DefaultCellStyle.SelectionForeColor = Color.Black;
                }
            }
        }


        // displaying data by day
        private void kryptonMonthCalendar1_DateChanged(object sender, DateRangeEventArgs e)
        {
            DateTime selectedDate = kryptonMonthCalendar1.SelectionStart;

            dataTable.DefaultView.RowFilter = "";

            dataTable.DefaultView.RowFilter = $"Datedone = '{selectedDate.ToString("yyyy-MM-dd")}'";
        }



        private void InitializeEvent()
        {
            AttachEvent(labelAll);
            AttachEvent(pictureBox1);

            AttachEvent(labelComplete);
            AttachEvent(pictureBox2);

            AttachEvent(labelPending);
            AttachEvent(pictureBox3);

            AttachEvent(pictureBoxClose);
            AttachEvent(pictureBoxMax);
            AttachEvent(pictureBoxMin);
        }

        private void AttachEvent(Control control)
        {
            control.MouseDown += (sender, e) => ControlMouseDown(sender, e, control);
            control.MouseEnter += (sender, e) => ControlMouseEnter(sender, e, control);
            control.MouseLeave += (sender, e) => ControlMouseLeave(sender, e, control);
            control.MouseUp += (sender, e) => ControlMouseUp(sender, e, control);
        }

        private void ControlMouseDown(object sender, MouseEventArgs e, Control control)
        {
            if (control == labelAll || control == pictureBox1)
            {
                labelAll.BorderStyle = BorderStyle.Fixed3D;
                pictureBox1.BorderStyle = BorderStyle.Fixed3D;
            }
            if (control == labelComplete || control == pictureBox2)
            {
                labelComplete.BorderStyle = BorderStyle.Fixed3D;
                pictureBox2.BorderStyle = BorderStyle.Fixed3D;
            }
            if (control == labelPending || control == pictureBox3)
            {
                labelPending.BorderStyle = BorderStyle.Fixed3D;
                pictureBox3.BorderStyle = BorderStyle.Fixed3D;
            }

        }

        private void ControlMouseEnter(object sender, EventArgs e, Control control)
        {
            if (control == labelAll || control == pictureBox1)
            {
                labelAll.BackColor = Color.BurlyWood;
                pictureBox1.BackColor = Color.BurlyWood;
            }
            if (control == labelComplete || control == pictureBox2)
            {
                labelComplete.BackColor = Color.BurlyWood;
                pictureBox2.BackColor = Color.BurlyWood;
            }
            if (control == labelPending || control == pictureBox3)
            {
                labelPending.BackColor = Color.BurlyWood;
                pictureBox3.BackColor = Color.BurlyWood;
            }
            if (control == pictureBoxClose)
            {
                pictureBoxClose.BackColor = Color.OrangeRed;
            }
            if (control == pictureBoxMax)
            {
                pictureBoxMax.BackColor = Color.Bisque;
            }
            if (control == pictureBoxMin)
            {
                pictureBoxMin.BackColor = Color.Bisque;
            }
        }

        private void ControlMouseLeave(object sender, EventArgs e, Control control)
        {
            if (control == labelAll || control == pictureBox1)
            {
                labelAll.BackColor = Color.FromArgb(255, 240, 200);
                pictureBox1.BackColor = Color.FromArgb(255, 240, 200);
            }
            if (control == labelComplete || control == pictureBox2)
            {
                labelComplete.BackColor = Color.FromArgb(255, 240, 200);
                pictureBox2.BackColor = Color.FromArgb(255, 240, 200);
            }
            if (control == labelPending || control == pictureBox3)
            {
                labelPending.BackColor = Color.FromArgb(255, 240, 200);
                pictureBox3.BackColor = Color.FromArgb(255, 240, 200);
            }
            if (control == pictureBoxClose)
            {
                pictureBoxClose.BackColor = Color.BurlyWood;
            }
            if (control == pictureBoxMax)
            {
                pictureBoxMax.BackColor = Color.BurlyWood;
            }
            if (control == pictureBoxMin)
            {
                pictureBoxMin.BackColor = Color.BurlyWood;
            }
        }

        private void ControlMouseUp(object sender, MouseEventArgs e, Control control)
        {
            if (control == labelAll || control == pictureBox1)
            {
                labelAll.BorderStyle = BorderStyle.None;
                pictureBox1.BorderStyle = BorderStyle.None;
            }
            if (control == labelComplete || control == pictureBox2)
            {
                labelComplete.BorderStyle = BorderStyle.None;
                pictureBox2.BorderStyle = BorderStyle.None;
            }
            if (control == labelPending || control == pictureBox3)
            {
                labelPending.BorderStyle = BorderStyle.None;
                pictureBox3.BorderStyle = BorderStyle.None;
            }
        }
    }
}
