using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Emc.Documentum.Rest.DataModel;
using Emc.Documentum.Rest.Http.Utility;
using Emc.Documentum.Rest.Net;
using System.IO;

namespace AspNetWebFormsRestConsumer
{
    public partial class Contact : Page
    {
        

        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected Repository GetRepository()
        {
            RestController client;
            RestService home;
            ProductInfo productInfo;

            client = new RestController("dmadmin", "D3m04doc!");
            home = client.Get<RestService>("http://10.8.76.108:7070/D2-REST_4.6.0/services", null);
            home.SetClient(client);
            productInfo = home.GetProductInfo();
            return home.GetRepository("repo1");
        }

        protected void btnCreate_Click(object sender, EventArgs e)
        {
            RestDocument doc = null;
            Repository repository = GetRepository();
            if (String.IsNullOrEmpty(txtName.Text))
            {
                lblError.Visible = true;
                lblError.Text = "Name is required";
                lblError.ForeColor = System.Drawing.Color.Red;
            }
            else
            {
                lblError.Text = "";
                lblError.Visible = false;
                if (txtPathOrProfile.Text.Contains("/"))
                {
                    lblError.Visible = true;
                    lblError.Text = "Is a path";
                    Folder saveToFolder = repository.getOrCreateFolderByPath(txtPathOrProfile.Text);

                    if (!String.IsNullOrEmpty(fileToUpload.FileName))
                    {
                        string trailingPath = Path.GetFileName(fileToUpload.PostedFile.FileName);
                        string fullPath = Path.Combine(Server.MapPath(" "), trailingPath);
                        fileToUpload.SaveAs(fullPath);
                        FileInfo tmpFile = new FileInfo(fullPath);
                        doc = repository.ImportNewDocument(tmpFile, txtName.Text, txtPathOrProfile.Text);
                        doc.setAttributeValue("object_name", txtName.Text);
                        doc.setAttributeValue("title", txtTitle.Text);
                        doc.setAttributeValue("subject", txtSubject.Text);
                        doc.Save();
                        doc = doc.fetch<RestDocument>();
                        lblError.ForeColor = System.Drawing.Color.Green;
                        lblError.Text = "Saved: \n" + doc.ToString();
                        tmpFile.Delete();
                    }
                }
                else
                {
                    lblError.Visible = true;
                    lblError.Text = "Is a profile";
                }      
            }
        }
    }
}