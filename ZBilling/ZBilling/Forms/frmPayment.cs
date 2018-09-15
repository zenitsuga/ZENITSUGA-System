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
    public partial class frmPayment : Form
    {
        public string DBPath;
        clsFunctiion cf = new clsFunctiion();

        public string LoginUser;

        DataView dvRecords;
        DataTable DTRecordQuery;

        public frmPayment()
        {
            InitializeComponent();
        }

        private DataTable getRecordsForQuery(string CustType)
        {
            DataTable dtResult = new DataTable();
            try
            {
                string Query = "Select sysid,LastName,FirstName,MiddleName,Address,ContactNumber from " + CustType + " order by LastName asc";
                dtResult = cf.GetRecords(Query);
            }
            catch
            {
            }
            if (dtResult.Rows.Count > 0)
            {
                dvRecords = dtResult.DefaultView;
            }
            return dtResult;
        }


        private void frmPayment_Load(object sender, EventArgs e)
        {
            cf.DbLocation = DBPath;
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton1.Checked)
            {
                DTRecordQuery = getRecordsForQuery("tblCustomerTenant");
            }
            dataGridView1.DataSource = dvRecords.ToTable();
        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton2.Checked)
            {
                DTRecordQuery = getRecordsForQuery("tblTenant");
            }
            dataGridView1.DataSource = dvRecords.ToTable();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            try
            {   
                dvRecords.RowFilter = "Lastname Like '" + textBox1.Text + "%'";
                if (dvRecords.ToTable().Rows.Count == 0)
                {
                    dvRecords.RowFilter = "Firstname Like '" + textBox1.Text + "%'";
                    if (dvRecords.ToTable().Rows.Count == 0)
                    {
                        dvRecords.RowFilter = "Middlename Like '" + textBox1.Text + "%'";
                    }
                }

                if (textBox1.Text == string.Empty)
                {
                    dvRecords.RowFilter = null;
                }

                dataGridView1.DataSource = dvRecords.ToTable();
            }
            catch
            {

            }
        }

        private string GetBilledCustomer(string ID)
        {
            string Result = "N/A";
            try
            {
                string TableName = string.Empty;
                TableName = radioButton1.Checked  ? "tblCustomerTenant" : "tblTenant";
                TableName = "tblTenant";
                string Query = "Select LastName + ',' + FirstName + ' ' + MiddleName from "+ TableName +" where sysID=" + ID;
                Result = cf.GetRecords(Query).Rows[0][0].ToString();
                tssBilledTo.Text = Result;
            }
            catch
            {
            }
            return Result;
        }

        private void dataGridView1_KeyPress(object sender, KeyPressEventArgs e)
        {
            ClearMe();
            string CustID = "0";
            string CustType = string.Empty;

            comboBox1.DataSource = null;

            if (e.KeyChar == 13)
            {
                //dataGridView1.CurrentCell = dataGridView1[0, dataGridView1.CurrentCell.RowIndex - 1];
                dataGridView1.CurrentRow.Selected = true;
                if (radioButton1.Checked)
                {
                    frmSelectTenant st = new frmSelectTenant();
                    st.DBPath = DBPath;
                    st.OwnerID = dataGridView1[0, dataGridView1.CurrentCell.RowIndex].Value.ToString();
                    st.ShowDialog();
                    CustType = "TenantID";
                    CustID = st.SelectedTenantID;

                }
                else
                {
                    CustType = "CustomerID";
                    CustID = dataGridView1[0, dataGridView1.CurrentCell.RowIndex].Value.ToString();
                }
                comboBox1.DataSource = cf.GetRecords("Select RoomNumber from tblRoomAssignment where " + CustType + " =" + CustID);
                comboBox1.DisplayMember = "RoomNumber";
                GetBilledCustomer(CustID);
            }
        }

        private void LoadBilling()
        {
            try
            {
                string Query = "select t.sysid,t.DateTransaction,t.transactionNo,td.Description, " +
                               " td.Amount,t.RoomID,td.Remarks,td.isPaid " +
                               " from tbltransactiondetails td " +
                               " left join tbltransaction t on td.ReferenceID = t.sysid " +
                               " where td.isPaid = 0 and t.RoomId =" + comboBox1.Text;
                DataTable dtQuery = cf.GetRecords(Query);

                double Amount = 0.00;
                
                if (dtQuery.Rows.Count > 0)
                {
                    dataGridView2.DataSource = dtQuery;

                    foreach (DataGridViewRow dgrv in dataGridView2.Rows)
                    {
                        if(!string.IsNullOrEmpty(dgrv.Cells["Amount"].Value.ToString()))
                        {
                            Amount += double.Parse(dgrv.Cells["Amount"].Value.ToString());
                        }
                    }
                    tssTotalAmount.Text = string.Format("{0:C}",Amount).Replace("$","");
                }
            }
            catch
            {
            }
        }

        private void ClearMe()
        {
            try
            {
                dataGridView2.DataSource = null;
                tssTotalAmount.Text = "0.00";
                tssBilledTo.Text = "N/A";
            }
            catch
            {
            }
        }

        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                ClearMe();
                string CustID = "0";
                string CustType = string.Empty;

                    //dataGridView1.CurrentCell = dataGridView1[0, dataGridView1.CurrentCell.RowIndex - 1];
                    dataGridView1.CurrentRow.Selected = true;
                    if (radioButton1.Checked)
                    {
                        frmSelectTenant st = new frmSelectTenant();
                        st.DBPath = DBPath;
                        st.OwnerID = dataGridView1[0, dataGridView1.CurrentCell.RowIndex].Value.ToString();
                        st.ShowDialog();
                        CustType = "TenantID";
                        CustID = st.SelectedTenantID;
                    }
                    else
                    {
                        CustType = "TenantID";
                        CustID = dataGridView1[0, dataGridView1.CurrentCell.RowIndex].Value.ToString();
                    }
                    comboBox1.DataSource = cf.GetRecords("Select RoomNumber from tblRoomAssignment where " + CustType + " =" + CustID);
                    comboBox1.DisplayMember = "RoomNumber";
                    GetBilledCustomer(CustID);
            }
            catch
            {
            }
        }

        private void comboBox1_SelectedValueChanged(object sender, EventArgs e)
        {
            LoadBilling();
        }

        private void payTransactionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            
        }

        private void payTransactionToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            if (tssTotalAmount.Text != "0.00" && tssBilledTo.Text != "N/A")
            {
                Form1 f = new Form1();
                //f.MdiParent = ZBilling.Form1.ActiveForm;
                f.DBPath = DBPath;
                f.Amount = tssTotalAmount.Text;
                f.ShowDialog();
            }
        }
    }
}
