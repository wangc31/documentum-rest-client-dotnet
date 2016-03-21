using Emc.Documentum.Rest.DataModel;
using Emc.Documentum.Rest.Http.Utility;
using Emc.Documentum.Rest.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace AspNetWebFormsRestConsumer
{
    public partial class DQL : Page
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

        private void UpdateGrid(String qualifier)
        {

            Feed<RestDocument> feed = GetRepository().ExecuteDQL<RestDocument>("select * from " + qualifier,
                new FeedGetOptions() { Inline = true, Links = true });
            List<RestDocument> docs = feed == null? null : ObjectUtil.getFeedAsList<RestDocument>(feed);

            List<SimpleDocumentProperties> lst = new List<SimpleDocumentProperties>();
            foreach (RestDocument doc in docs)
            {
                SimpleDocumentProperties sdp = new SimpleDocumentProperties();
                sdp.Id = doc.getAttributeValue("r_object_id").ToString();
                sdp.Name = doc.getAttributeValue("object_name").ToString();
                sdp.Subject = doc.getAttributeValue("subject").ToString();
                sdp.Title = doc.getAttributeValue("title").ToString();
                String folderId = doc.getRepeatingString("i_folder_id", 0);
                Folder folder = GetRepository().getObjectById<Folder>(folderId);
                String folderPath = folder.getRepeatingString("r_folder_path", 0);
                sdp.FolderPath = folderPath;
                lst.Add(sdp);
            }
            gridView.DataSource = lst;
            gridView.DataBind();
        }



        protected void gridView_RowEditing(object sender, GridViewEditEventArgs e)
        {
            //Set the edit index.
            gridView.EditIndex = e.NewEditIndex;
            //Bind data to the GridView control.
            UpdateGrid(null);
        }

        protected void gridView_RowCancelingEdit(object sender, GridViewCancelEditEventArgs e)
        {
            //Reset the edit index.
            gridView.EditIndex = -1;
            //Bind data to the GridView control.
            UpdateGrid(null);
        }

        protected void gridView_RowUpdating(object sender, GridViewUpdateEventArgs e)
        {

            //Reset the edit index.
            gridView.EditIndex = -1;
        }

        protected void btnExecuteQuery_Click(object sender, EventArgs e)
        {
            
            String qualifier = txtQuery.Text;
            if (qualifier.StartsWith("select"))
            {
                qualifier = qualifier.Substring(qualifier.ToLower().IndexOf("from ")+5);
            }
            UpdateGrid(qualifier);
        }
    }

    class SimpleDocumentProperties
    {
        public String Id { get; set; }
        public String Name { get; set; }
        public String Subject { get; set; }
        public String Title { get; set; }
        public String FolderPath { get; set; }

    }
}