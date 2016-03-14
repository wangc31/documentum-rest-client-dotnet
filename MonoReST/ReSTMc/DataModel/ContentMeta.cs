using Emc.Documentum.Rest.Net;
using Emc.Documentum.Rest.Http.Utility;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Collections.Specialized;
using System.Runtime.InteropServices;

namespace Emc.Documentum.Rest.DataModel
{

    /// <summary>
    /// 
    /// </summary>
    [DataContract(Name = "content", Namespace = "http://identifiers.emc.com/vocab/documentum")]
    [ClassInterface(ClassInterfaceType.AutoDual)]
    public class ContentMeta : PersistentObject
    {
        /// <summary>
        /// Get the associated document resource of this content
        /// </summary>
        /// <param name="options"></param>
        /// <returns>Returns a RestDocument object </returns>
        public RestDocument GetParentDocument(SingleGetOptions options)
        {
            return Client.GetSingleton<RestDocument>(this.Links, LinkUtil.PARENT.Rel, options);
        }

        /// <summary>
        /// Download the content media of this content to a local file
        /// </summary>
        /// <returns>Returns System FileInfo</returns>
        public FileInfo DownloadContentMedia()
        {
            
            string contentMediaUri = LinkUtil.FindLinkAsString(this.Links, LinkUtil.CONTENT_MEDIA.Rel);
            string fileName = (string)getAttributeValue("object_name"); 
            string fileExtension = (string)getAttributeValue("dos_extension");
            if (String.IsNullOrEmpty(fileName))
            {
                fileName = "temp-" + System.Guid.NewGuid().ToString();
            }
            if (String.IsNullOrEmpty(fileExtension))
            {
                fileExtension = fileName.Substring(fileName.LastIndexOf('.') + 1);
                if(fileName.Equals(fileExtension))
                    fileExtension = "txt";
            }
            fileName = ObjectUtil.getSafeFileName(fileName);
            string fullPath = Path.Combine(Path.GetTempPath(), fileName + "." + fileExtension);

            using (Stream media = Client.GetRaw(contentMediaUri))
            {
                FileStream fs = File.Create(fullPath);
                media.CopyTo(fs);
                fs.Dispose();
            }

            return new FileInfo(fullPath);
        }

        public String getMediaUri()
        {
            return LinkUtil.FindLinkAsString(this.Links, LinkUtil.CONTENT_MEDIA.Rel);
        }
    }
}
