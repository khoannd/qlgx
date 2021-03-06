using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using GxGlobal;
using Femiani.Forms.UI.Input;
using System.Xml;

namespace GxControl
{
    public partial class GxTextField : GxBaseField
    {
        private AutoCompleteTextBox txt = new AutoCompleteTextBox();
        public const string XML_AUTOCOMPLETE_FILENAME = "autocomplete.xml";
        public static XmlDocument xmlAutoComplete = null;
        private static string path_xml = Memory.AppPath + XML_AUTOCOMPLETE_FILENAME;
        Button acceptButton = null;
        public GxTextField()
        {
            InitializeComponent();
            txt.Leave += new EventHandler(txt_Leave);
            txt.Enter += new EventHandler(txt_Enter);
            txt.Resize += new EventHandler(txt_Resize);
            label1.Click += Label1_Click;
            this.Load += new EventHandler(GxTextField_Load);
            InitControl();
            if (xmlAutoComplete == null && !Memory.IsDesignMode)
            {
                CreateXmlFile(path_xml);
                xmlAutoComplete = new XmlDocument();
                xmlAutoComplete.Load(path_xml);
            }
        }

        private void Label1_Click(object sender, EventArgs e)
        {
            txt.Focus();
        }

        private static void CreateXmlFile(string path_xml)
        {
            //create template xml file if not existed
            if (!System.IO.File.Exists(path_xml))
            {
                System.IO.StreamWriter sw = new System.IO.StreamWriter(path_xml, false, Encoding.UTF8);
                sw.WriteLine("<?xml version=\"1.0\" encoding=\"utf-8\" ?>");
                sw.WriteLine("<application value=\"GiaoXu\" xmlns=\"urn:autocomplete-schema\">");
                sw.WriteLine("  <forms>");
                sw.WriteLine("      <frmMain>");
                sw.WriteLine("          <controls>");
                sw.WriteLine("          </controls>");
                sw.WriteLine("      </frmMain>");
                sw.WriteLine("  </forms>");
                sw.WriteLine("</application>");
                sw.Close();
            }
        }

        private bool numberInputRequired = true;

        public bool NumberInputRequired
        {
            get { return numberInputRequired; }
            set { numberInputRequired = value; }
        }

        public int MaxLength
        {
            get { return txt.MaxLength; }
            set { txt.MaxLength = value; }
        }

        public AutoCompleteTextBox TextBox
        {
            get { return txt; }
        }

        private bool numberMode = false;

        public bool NumberMode
        {
            get { return numberMode; }
            set { numberMode = value; }
        }

        public bool ReadOnly
        {
            get { return txt.ReadOnly; }
            set { txt.ReadOnly = value; }
        }

        public bool MultiLine
        {
            get { return txt.Multiline; }
            set
            {
                txt.Multiline = value;
                //if (value == true) ChangeSize();
            }
        }

        public bool AutoCompleteEnabled
        {
            get { return txt.AutoCompleteEnabled; }
            set { txt.AutoCompleteEnabled = value; }
        }

        protected override void InitControl()
        {
            editBase = txt;
            base.InitControl();
        }

        private void txt_Leave(object sender, EventArgs e)
        {
            if (numberMode && numberInputRequired)
            {
                if (!Validator.IsNumber(txt.Text))
                {
                    txt.Text = "0";
                }
            }
            if (MultiLine && acceptButton != null)
            {
                Form f = this.FindForm();
                if (f != null)
                {
                    f.AcceptButton = acceptButton;
                }
                acceptButton = null;
            }
        }

        private void txt_Enter(object sender, EventArgs e)
        {
            if (MultiLine)
            {
                Form f = this.FindForm();
                if (f != null)
                {
                    acceptButton = (Button)f.AcceptButton;
                    f.AcceptButton = null;
                }
            }
        }

        private void GxTextField_Load(object sender, EventArgs e)
        {
            if (AutoCompleteEnabled)
            {
                txt.PopupBorderStyle = BorderStyle.FixedSingle;
                Form frmParent = this.FindForm();
                if (frmParent != null)
                {
                    frmParent.FormClosing += new FormClosingEventHandler(frmParent_FormClosing);
                    this.TextBox.Items = GetAutoCompleteItems(frmParent.Name, this.Name);
                }
            }
        }

        private void txt_Resize(object sender, EventArgs e)
        {
            txt.PopupWidth = txt.Width;
        }

        private void frmParent_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (AutoCompleteEnabled)
            {
                SaveAutoCompleteItem((sender as Form).Name, this.Name, Label, this.Text);
            }
        }

        public static AutoCompleteEntryCollection GetAutoCompleteItems(string formName, string controlName)
        {
            AutoCompleteEntryCollection items = new AutoCompleteEntryCollection();
            try
            {
                if (xmlAutoComplete != null)
                {
                    // Create an XmlNamespaceManager to resolve the default namespace.
                    XmlNamespaceManager nsmgr = new XmlNamespaceManager(xmlAutoComplete.NameTable);
                    nsmgr.AddNamespace("autocp", "urn:autocomplete-schema");

                    XmlElement root = xmlAutoComplete.DocumentElement;
                    XmlNode forms = root.SelectSingleNode("/autocp:application/autocp:forms", nsmgr);
                    if (forms != null && forms.ChildNodes.Count > 0)
                    {
                        XmlNodeList nodeList = forms[formName].ChildNodes;
                        if (nodeList != null)
                        {
                            for (int i = 0; i < nodeList.Count; i++)
                            {
                                XmlNode control = nodeList[i];
                                if (control.Name == controlName)
                                {
                                    foreach (XmlNode node in control.ChildNodes)
                                    {
                                        items.Add(new AutoCompleteEntry(node.InnerText.Trim(), node.InnerText.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)));
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch
            {
            }
            return items;
        }

        public static void SaveAutoCompleteItem(string formName, string controlName, string caption, string value)
        {
            try
            {
                if (xmlAutoComplete != null && value.Trim() != "")
                {
                    // Create an XmlNamespaceManager to resolve the default namespace.
                    XmlNamespaceManager nsmgr = new XmlNamespaceManager(xmlAutoComplete.NameTable);
                    nsmgr.AddNamespace("autocp", "urn:autocomplete-schema");

                    XmlElement root = xmlAutoComplete.DocumentElement;
                    //select forms
                    XmlNode nodeForms = root.SelectSingleNode(string.Format("/autocp:application/autocp:forms", formName), nsmgr);
                    if (nodeForms != null)
                    {
                        //select input form
                        XmlElement nodeForm = nodeForms[formName];
                        XmlNode nodeControl = null;
                        if (nodeForm == null)
                        {
                            //if not existed, create new form node
                            nodeForm = xmlAutoComplete.CreateElement(formName);
                            nodeForms.AppendChild(nodeForm);

                        }
                        //select input control
                        nodeControl = nodeForm[controlName];
                        if (nodeControl == null)
                        {
                            //add new control to new form node
                            nodeControl = xmlAutoComplete.CreateElement(controlName);
                            nodeForm.AppendChild(nodeControl);
                        }
                        XmlAttribute captionAtt = nodeControl.Attributes["caption"];
                        if (captionAtt == null)
                        {
                            captionAtt = xmlAutoComplete.CreateAttribute("caption");
                            nodeControl.Attributes.Append(captionAtt);
                        }
                        captionAtt.Value = caption;

                        //find value item
                        XmlNodeList nodeList = nodeControl.ChildNodes;
                        XmlNode nodeFound = null;
                        foreach (XmlNode itemNode in nodeList)
                        {
                            if (itemNode.InnerText.ToLower() == value.Trim().ToLower())
                            {
                                nodeFound = itemNode;
                                break;
                            }
                        }
                        //if not found
                        if (nodeFound == null)
                        {
                            XmlElement newItemNode = xmlAutoComplete.CreateElement("item");
                            newItemNode.InnerXml = value.Trim();
                            nodeControl.AppendChild(newItemNode);
                        }
                    }
                    xmlAutoComplete.Save(path_xml);
                }
            }
            catch { }
        }
    }
}