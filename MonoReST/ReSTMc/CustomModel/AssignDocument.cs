using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Emc.Documentum.Rest.CustomModel
{
    /// <summary>
    /// Keeps track of which documents go to which case/requests
    /// </summary>
    public class AssignDocument
    {
        /// <summary>
        /// The ID of the document. This ID will change/update when the document is assigned to a case/request as assigning is a copy operation
        /// </summary>
        public string DocumentId { get; set; }
        /// <summary>
        /// The caseId the document will be assigned to
        /// </summary>
        public string CaseId { get; set; }
        /// <summary>
        /// The requestId the document will be assigned to
        /// </summary>
        public string RequestId { get; set; }
        /// <summary>
        /// Not used, the assisId the document will be assigned to
        /// </summary>
        public string AssistId { get; set; }

        /// <summary>
        /// Constructor used to assign a document to a case/request/assistId
        /// </summary>
        /// <param name="documentId"></param>
        /// <param name="caseId"></param>
        /// <param name="requestId"></param>
        /// <param name="assistId"></param>
        public AssignDocument(string documentId, string caseId, string requestId, string assistId) {
            DocumentId = documentId;
            CaseId = caseId;
            RequestId = requestId;
            AssistId = assistId;
        }

        /// <summary>
        /// Constuctor for assigning a document to a case/request
        /// </summary>
        /// <param name="documentId"></param>
        /// <param name="caseId"></param>
        /// <param name="requestId"></param>
        public AssignDocument(string documentId, string caseId, string requestId)
        {
            DocumentId = documentId;
            CaseId = caseId;
            RequestId = requestId;
            AssistId = null;
        }

        /// <summary>
        /// Constructor for assigning a document to a case
        /// </summary>
        /// <param name="documentId"></param>
        /// <param name="caseId"></param>
        public AssignDocument(string documentId, string caseId)
        {
            DocumentId = documentId;
            CaseId = caseId;
            RequestId = null;
            AssistId = null;
        }

        /// <summary>
        /// Returns the relative path "case/request" vs. "/SystemA/Process/case/request" 
        /// for the assignment
        /// </summary>
        /// <returns> String that is the path</returns>
        public string getPath()
        {
            StringBuilder pathBuilder = new StringBuilder();
            if (CaseId != null && !CaseId.Trim().Equals(""))
            {
                pathBuilder.Append(CaseId);
                if (RequestId != null && !RequestId.Trim().Equals(""))
                {
                    pathBuilder.Append("/" + RequestId);
                    if (AssistId != null && !AssistId.Trim().Equals(""))
                    {
                        pathBuilder.Append("/" + AssistId);
                    }
                }
            }
            return pathBuilder.ToString();
        }
    }
}
