﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ZBilling.Class;

namespace ZBilling.Forms
{
    public partial class frmRoomAssignment : Form
    {
        public string DBPath;
        public string LoginUser;

        DataView dvResult;

        clsFunctiion cf = new clsFunctiion();
        public frmRoomAssignment()
        {
            InitializeComponent();
        }

        private void searchRecordsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            radioButton1.Select();
            SearchRecord sr = new SearchRecord();
            sr.DBPath = DBPath;
            sr.TableName = "tblCustomerTenant";
            sr.Criteria = " where isActive = 1";
            sr.FieldOutput = "sysid,Lastname,Firstname,middlename,Address,ContactNumber,(Lastname + ',' + Firstname + ' ' + middlename) as TagOutput";
            sr.ShowDialog();
            if (sr.Result != null)
            {
                TextBox tb = (TextBox)sr.Result;
                string sysID = tb.Text;
                textBox1.Text = tb.Tag.ToString();
                textBox2.Text = sysID;
                LoadRoomAssignment(sysID);
                comboBox1.Focus();
            }
        }

        private void frmRoomAssignment_Load(object sender, EventArgs e)
        {
            string CustType = string.Empty;
            //CustType = radioButton1.Checked ? "Owner" : "Tenant";
            LoadRoomAssignment(CustType);
        }

        private void groupBox2_Enter(object sender, EventArgs e)
        {

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(textBox2.Text))
            {
                MessageBox.Show("Cannot assign room. Please check your Owner/Tenant Name.", "No Owner/Tenant Name select", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            SaveRoomAssignment(textBox2.Text);
        }
        private DataTable dtRoomInfo()
        {
            DataTable dtResult = new DataTable();
            try
            {
                cf.DbLocation = DBPath;
                string SQLStatement = "Select * from tblRooms where isActive = 1";
                dtResult = cf.GetRecords(SQLStatement);
            }
            catch
            {
            }
            return dtResult;
        }
        private DataTable GetRoomType(DataTable dtRooms)
        {
            DataTable dtResult = new DataTable();
            try
            {
                DataView dv = dtRooms.DefaultView;
                dtResult = dv.ToTable(true, "RoomType");
            }
            catch
            {
            }
            return dtResult;
        }
        private void LoadRoomAssignment(string SysID)
        {
            try
            {
                string filterID = radioButton1.Checked ? "c.sysid" : "t.sysid";
                filterID = radioButton2.Checked ? "t.sysid" : "c.sysid";
                dataGridView1.DataSource = null;
                string SQLStatement =" SELECT r.RoomNumber, " +
                                     "case when ((isnull(c.LastName,'') + ',' + isnull(c.FirstName,''))) = ',' then "+
                                     "(isnull(ct.LastName,'') + ',' + isnull(ct.FirstName,'')) " +
                                     " else (isnull(c.LastName,'') + ',' + isnull(c.FirstName,'')) end as OwnerName, " +
                                     "case when ((isnull(t.LastName,'') + ',' + isnull(t.FirstName,''))) = ',' then '' else (isnull(t.LastName,'') + ',' + isnull(t.FirstName,'')) end as TenantName,  " +
                                     "r.dateTransferred " +
                                     "from tblRoomAssignment r " + 
                                     "left join tblCustomerTenant c on r.CustomerID = c.sysid " + 
                                     "left join tblTenant t on r.TenantID = t.sysid " +
                                     "left join tblCustomerTenant ct on t.OwnerID = ct.sysid " +
                                     "where r.isActive = 1 " +  (string.IsNullOrEmpty(SysID) ? "":" and " + filterID + "=" +SysID);
                cf.DbLocation = DBPath;
                DataTable dtResult = cf.GetRecords(SQLStatement);
                dataGridView1.DataSource = dtResult;
                if (dtResult.Rows.Count > 0)
                {
                    dvResult = new DataView(dtResult);
                }
                dataGridView1.Refresh();

                DataTable dtRooms = dtRoomInfo();
                comboBox1.DataSource = GetRoomType(dtRooms);
                comboBox1.DisplayMember = "RoomType";
            }
            catch
            {
            }
        }
        public void SaveRoomAssignment(string CustID)
        {
            try
            {
                cf.DbLocation = DBPath;
                int UserID = cf.GetSysID("tblUsers", " where username='" + LoginUser + "'");
                string IDType = radioButton1.Checked ? "CustomerID" : "TenantID";
                IDType = radioButton2.Checked ? "TenantID" : "CustomerID";
                string SQLStatement = "Insert into tblRoomAssignment(RoomNumber,"+ IDType +",DateTransferred,Remarks,UserID)values(" + comboBox2.Text + "," + CustID + ",'" + dateTimePicker1.Value.ToString("yyyy-MM-dd") + "','" + textBox5.Text + "'," + UserID + ")";
                if (cf.ExecuteNonQuery(SQLStatement))
                {
                    MessageBox.Show("Done Room assigning.", "Room assigning", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    textBox1.Text = string.Empty;
                    textBox2.Text = string.Empty;
                    textBox5.Text = string.Empty;
                    textBox3.Text = string.Empty;
                    comboBox1.Text = string.Empty;
                    comboBox2.Text = string.Empty;
                    LoadRoomAssignment(string.Empty);
                }
                else
                {
                    MessageBox.Show("Error: Cannot assign room", "Room Assignment Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                //LoadRoomAssignment(lblIDNumber.Text);
            }
            catch
            {
            }
        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {
            try
            {
                if (dvResult.ToTable().Rows.Count > 0)
                {
                    dvResult.RowFilter = string.Format("CONVERT(RoomNumber,System.String) like '" + textBox3.Text + "%'");
                    if (dvResult.ToTable().Rows.Count == 0)
                    {
                        dvResult.RowFilter = string.Format("CONVERT(OwnerName,System.String) like '" + textBox3.Text + "%'");
                        if (dvResult.ToTable().Rows.Count == 0)
                        {
                            dvResult.RowFilter = string.Format("CONVERT(TenantName,System.String) like '" + textBox3.Text + "%'");
                        }   
                    }
                }
                dataGridView1.DataSource = null;
                dataGridView1.DataSource = dvResult;
                dataGridView1.Refresh();
                if (dataGridView1.Rows.Count == 0)
                {
                    LoadRoomAssignment(string.Empty);
                }
            }
            catch
            {
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            try
            {
                frmRooms rm = new frmRooms();
                rm.MdiParent = Form1.ActiveForm;
                rm.DBPath = DBPath;
                rm.LoginUser = LoginUser;
                rm.Show();

            }
            catch
            {
            }
        }
        private DataTable GetRoomNumber(string Roomtype)
        {
            DataTable dtResult = new DataTable();
            try
            {
                DataTable dtRooms = dtRoomInfo();
                DataView dv = dtRooms.DefaultView;
                dv.RowFilter = "RoomType='" + Roomtype + "'";
                dtResult = dv.ToTable();
            }
            catch
            {
            }
            return dtResult;
        }
        private void comboBox1_SelectedValueChanged(object sender, EventArgs e)
        {
            comboBox2.DataSource = GetRoomNumber(comboBox1.Text);
            comboBox2.DisplayMember = "RoomName";
        }

        private void loadAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LoadRoomAssignment(string.Empty);
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            try
            {
                LoadRoomAssignment(textBox2.Text);
            }
            catch
            {
            }
        }

        private void tenantRecordsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            radioButton2.Select();
            SearchRecord sr = new SearchRecord();
            sr.DBPath = DBPath;
            sr.TableName = "tblTenant";
            sr.Criteria = " where isActive = 1";
            sr.FieldOutput = "sysid,Lastname,Firstname,middlename,Address,ContactNumber,(Lastname + ',' + Firstname + ' ' + middlename) as TagOutput";
            sr.ShowDialog();
            if (sr.Result != null)
            {
                TextBox tb = (TextBox)sr.Result;
                string sysID = tb.Text;
                textBox1.Text = tb.Tag.ToString();
                textBox2.Text = sysID;
                LoadRoomAssignment(sysID);
                comboBox1.Focus();
            }
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton1.Checked)
            {
                radioButton1.Select();
                SearchRecord sr = new SearchRecord();
                sr.DBPath = DBPath;
                sr.TableName = "tblCustomerTenant";
                sr.Criteria = " where isActive = 1";
                sr.FieldOutput = "sysid,Lastname,Firstname,middlename,Address,ContactNumber,(Lastname + ',' + Firstname + ' ' + middlename) as TagOutput";
                sr.ShowDialog();
                if (sr.Result != null)
                {
                    TextBox tb = (TextBox)sr.Result;
                    string sysID = tb.Text;
                    textBox1.Text = tb.Tag.ToString();
                    textBox2.Text = sysID;
                    LoadRoomAssignment(sysID);
                    comboBox1.Focus();
                }
            }
        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton2.Checked)
            {
                radioButton2.Select();
                SearchRecord sr = new SearchRecord();
                sr.DBPath = DBPath;
                sr.TableName = "tblTenant";
                sr.Criteria = " where isActive = 1";
                sr.FieldOutput = "sysid,Lastname,Firstname,middlename,Address,ContactNumber,(Lastname + ',' + Firstname + ' ' + middlename) as TagOutput";
                sr.ShowDialog();
                if (sr.Result != null)
                {
                    TextBox tb = (TextBox)sr.Result;
                    string sysID = tb.Text;
                    textBox1.Text = tb.Tag.ToString();
                    textBox2.Text = sysID;
                    LoadRoomAssignment(sysID);
                    comboBox1.Focus();
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            LoadRoomAssignment(String.Empty);
        }

        private void dataGridView1_CellMouseDown(object sender, DataGridViewCellMouseEventArgs e)
        {
            switch (e.Button)
            {
                case MouseButtons.Right:
                    {
                        var relativeMousePosition = dataGridView1.PointToClient(Cursor.Position);
                        contextMenuStrip1.Show(dataGridView1, relativeMousePosition);
                    }
                    break;
            }
        }

        private void cToolStripMenuItem_Click(object sender, EventArgs e)
        {
            RemoveRoomAssignment( dataGridView1["RoomNumber",dataGridView1.CurrentCell.RowIndex].Value.ToString());
        }
        private void RemoveRoomAssignment(string sysID)
        {
            try
            {
                DialogResult dr = MessageBox.Show("Are you sure you want to removed Room Number " + sysID + "?", "Remove Assigned Room", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (dr == DialogResult.Yes)
                {
                    string Query = "Delete from tblRoomAssignment where RoomNumber=" + sysID;
                    cf.ExecuteNonQuery(Query);
                }
                button1.PerformClick();
            }
            catch
            {
            }
        }
    }
}
