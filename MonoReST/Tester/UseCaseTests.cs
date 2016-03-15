using Emc.Documentum.Rest.DataModel;
using Emc.Documentum.Rest.CustomModel;
using Emc.Documentum.Rest.Net;
using Emc.Documentum.Rest.Http.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Configuration;
using System.Collections.Specialized;
using System.Windows.Forms;
using System.Threading;
using Emc.Documentum.Rest.Utility;

namespace Emc.Documentum.Rest.Test
{
    /// <summary>
    /// 
    /// </summary>
    public class UseCaseTests : IDisposable
    {
        private ReSTController client;
        private string ReSTHomeUri;
        private string repositoryName;
        private bool printResult;
        private int numDocs;
        private int threadNum;
        private string tempPath;
        private DateTime testStart;
        private string testPrefix;
        string testDirectory = @"C:\Temp\";
        string randomEmailsDirectory = @"Test\emails";
        string randomFilesDirectory = @"Test";
        string renditionsDirectory = @"Renditions";
        string primaryContentDirectory = @"PrimaryContent";
        string ProcessBasePath = "/RestTester/TestFiles/";
        List<string> requestList = new List<string>();
        string caseId;
        bool openEachFile = false;
        bool showdownloadedfiles = false;
        Logger loggerForm = null;
        // Disposable.
        private bool _disposed;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="client"></param>
        /// <param name="ReSTHomeUri"></param>
        /// <param name="repositoryName"></param>
        /// <param name="printResult"></param>
        /// <param name="path"></param>
        /// <param name="ThreadNum"></param>
        /// <param name="numDocs"></param>
        public UseCaseTests(ReSTController client, string ReSTHomeUri, string repositoryName, bool printResult, string path, int ThreadNum, int numDocs)
        {
            this.client = client;
            this.ReSTHomeUri = ReSTHomeUri;
            this.repositoryName = repositoryName;
            this.printResult = printResult;
            this.tempPath = path;
            this.threadNum = ThreadNum;
            this.numDocs = numDocs;
            this.testStart = DateTime.Now;
            this.testPrefix = testStart.ToString("yyyyMMddhhmmss")+"-"+threadNum;
            this.caseId = "CASE-" + testPrefix; // new Random().Next(0, 5); ;
            client.Logger = new LoggerFacade("RestServices", "NA", caseId, caseId);
        }

        /// <summary>
        /// Unique list of SupportingDocuments (not Cases)
        /// </summary>
        /// <param name="request"></param>
        private void addSupportingDoc(string supportingDoc) {
			if(!requestList.Contains(supportingDoc)) {
				requestList.Add(supportingDoc);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public UseCaseTests()
        {

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="output"></param>
        private void WriteOutput(Object output) {
            string outtext = null;
            if (output is String)
            {
                outtext = (String)output;
                byte[] bytes = Encoding.ASCII.GetBytes(outtext);
                outtext = Encoding.ASCII.GetString(bytes);
                // Make sure there are no BEEPs left in the string
                outtext = outtext.Replace('\x0007', ' ');
                outtext = outtext.Replace("\t", "   ");
            }
            if (loggerForm != null)
            {
                Application.DoEvents();
                loggerForm.appendText(output + "\n");
            }
            else
            {
                Console.WriteLine(outtext == null? output : outtext);
            }
            File.AppendAllText(testDirectory + Path.DirectorySeparatorChar + "ProcessDocumentumTest.txt", DateTime.Now + "-" + output + "\n");
            if (loggerForm != null) Application.DoEvents();
        }

        public string getPathRelativeToExecutable(string path)
        {
			if (!path.Contains(":") && !path.StartsWith(""+Path.DirectorySeparatorChar))
            {
                string exeDirectory = AppDomain.CurrentDomain.BaseDirectory;
				if (path.StartsWith(""+Path.DirectorySeparatorChar))
                {
                    path = exeDirectory + path.Substring(1);
                }
                else
                {
                    path = exeDirectory + path;
                }
            }
            return path;
        }



        private void getPreferences()
        {
            bool useFormLogging = false;
			NameValueCollection testConfig = null;
			try {
				testConfig = ConfigurationManager.GetSection("restconfig") as NameValueCollection;
			} catch(ConfigurationErrorsException se) {
				Console.WriteLine("Configuration could  not load. If you are running under Visual Studio, ensure:\n" +
					"\n\"<section name=\"restconfig\" type=\"System.Configuration.NameValueSectionHandler\"/> is used. " +
					"\nIf running under Mono, ensure: " + 
					"\n<section name=\"restconfig\" type=\"System.Configuration.NameValueSectionHandler,System\"/> is used");
				Console.WriteLine ("\n\n" + se + "\n\n");
			}
            if (testConfig != null)
            {
                useFormLogging = Boolean.Parse(testConfig["useformlogging"].ToString());

                openEachFile = Boolean.Parse(testConfig["openeachfile"].ToString());
                showdownloadedfiles = Boolean.Parse(testConfig["showdownloadedfiles"].ToString());

                testDirectory = testConfig["testbasedirectory"];
                if (!testDirectory.EndsWith(Path.DirectorySeparatorChar + "")) testDirectory = testDirectory + Path.DirectorySeparatorChar + "";
                testDirectory = testDirectory + testPrefix;
                testDirectory = getPathRelativeToExecutable(testDirectory);
				Directory.CreateDirectory(testDirectory);
                // Setup logger and peroformance output
                string LogThreshold = testConfig["LogThreshold"].ToString();
                if (client.Logger != null)
                {
                    if (LogThreshold.ToLower().Trim().Equals("debug"))
                    {
                        client.Logger.LogLevelThreshold = LogLevel.DEBUG;
                        client.Logger.TestDirectory = testDirectory;
                        client.Logger.isPerformance = Boolean.Parse(testConfig["performancedatatoconsole"].ToString());
                    }
                }
                randomFilesDirectory = testConfig["randomfilesdirectory"].ToString();
                randomFilesDirectory = getPathRelativeToExecutable(randomFilesDirectory);

                randomEmailsDirectory = testConfig["randomemailsdirectory"].ToString();
                randomEmailsDirectory = getPathRelativeToExecutable(randomEmailsDirectory);

				renditionsDirectory = testDirectory + Path.DirectorySeparatorChar + "Renditions";
                primaryContentDirectory = testDirectory + Path.DirectorySeparatorChar + "PrimaryContent";
                if(!Directory.Exists(randomFilesDirectory))
                {
                    String msg = "Unable to find the directory specified: " + randomFilesDirectory + " to pull random content files from. Unable to proceed.";
                    WriteOutput(msg);
                    throw new Exception(msg);
                }
                if (!Directory.Exists(randomEmailsDirectory))
                {
                    String msg = "Unable to find the directory specified: " + randomEmailsDirectory + " to pull"
                        + " random email files from. If customized ReST email processing is installed, the"
                        + " customized email test will fail. This message can normally be ignored unless you"
                        + " have the custom ReST email adapter installed.";
                    WriteOutput(msg);
                }
                Directory.CreateDirectory(primaryContentDirectory);
                Directory.CreateDirectory(renditionsDirectory);
            }
            if(Type.GetType("Mono.Runtime") != null && useFormLogging && threadNum > 1)
            {
                WriteOutput("*** Form logging cannot be used for multiple threads on Mono, setting to false.");
                useFormLogging = false;
            }
            if (useFormLogging)
            {
                loggerForm = new Logger();
                loggerForm.setTitle(caseId);
                loggerForm.AutoSize = true;
                loggerForm.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
                int offset = threadNum == 0 ? threadNum : threadNum * 250;
                loggerForm.Top = threadNum + offset;
                loggerForm.Left = 0;
                loggerForm.getLoggerTextBox().Height = 250;
                loggerForm.getLoggerTextBox().Width = 1024;
                loggerForm.Show();
            }
            
        }

        /// <summary>
        /// 
        /// </summary>
        public void Start()
        {

                getPreferences();
                long testStart = DateTime.Now.Ticks;
                long tStart = DateTime.Now.Ticks;
                List<AssignDocument> assignDocs = null;
                List<string> idsWithRenditions = null;

                ReSTService home = client.Get<ReSTService>(ReSTHomeUri, null);
                if (home == null)
                {
                    WriteOutput("\nUnable to get Rest Service at: " + ReSTHomeUri + " check to see if the service is available.");
                    return;
                }
                home.SetClient(client);
				ProductInfo productInfo = home.GetProductInfo ();
                WriteOutput("Took " + ((DateTime.Now.Ticks - testStart)/TimeSpan.TicksPerMillisecond) + "ms to get ReSTService");
                //Feed<Repository> repositories = home.GetRepositories<Repository>(new FeedGetOptions { Inline = true, Links = true });
                //Repository repository = repositories.GetRepository(repositoryName);
                Repository repository = home.GetRepository(repositoryName);
                if (repository == null) throw new Exception("Unable to login to the repository, please see server logs for more details.");


                // Set our default folder and document types. 
                repository.DocumentType = "dm_document";
                repository.FolderType = "dm_folder";
                string testName = "";
                try {
                    // Import all documents into the holding area (Instantiation Form) before the documents are assigned to case/request
                    testName = "CreateTempDocs";
                    WriteOutput("-----BEGIN " + testName + "--------------");
                    tStart = DateTime.Now.Ticks;
                    assignDocs = CreateTempDocs(repository);
                    WriteOutput("Created " + numDocs + " in " + ((DateTime.Now.Ticks - tStart) / TimeSpan.TicksPerMillisecond) + "ms");
                    WriteOutput("-----FINISHED " + testName + "--------------");
                }
                catch (Exception e)
                {
                    WriteOutput("#####FAILED##### TEST [" + testName + "]" + e.StackTrace.ToString());
                }

                try {
                    // Assign Documents to case/supportingDocs
                    // When AssignDocs completes, the AssignDocument list will contain the new ObjectIds of each document 
                    // that was assigned to the case
                    testName = "AssignDocs";
                    WriteOutput("-----BEGIN " + testName + "-------------------------");
                    tStart = DateTime.Now.Ticks;
                    AssignToCaseSupportingDoc(repository, assignDocs);
                    WriteOutput("Assigned " + assignDocs.Count + " in " + ((DateTime.Now.Ticks - tStart) / TimeSpan.TicksPerMillisecond) + "ms");
                    WriteOutput("-----END " + testName + "---------------------------");
                }
                catch (Exception e)
                {
                    WriteOutput("#####FAILED##### TEST [" + testName + "]" + e.StackTrace.ToString());
                }

                try
                {
                    testName = "CreateFromTemplate";
                    WriteOutput("-----BEGIN " + testName + "-----------");
                    tStart = DateTime.Now.Ticks;
                    CreateFromTemplate(repository, assignDocs);
                    WriteOutput("Created 10 documents from template in " + ((DateTime.Now.Ticks - tStart) / TimeSpan.TicksPerMillisecond) + "ms");
                    WriteOutput("-----END " + testName + "-----------");
                }
                catch (Exception e)
                {
                    WriteOutput("#####FAILED##### TEST [" + testName + "]" + e.StackTrace.ToString());
                }

                try
                {
                    testName = "ReassignDocumentsToCase (Move documents)";
                    WriteOutput("-----BEGIN " + testName + "------------");
                    tStart = DateTime.Now.Ticks;
                    // Randomly take some assigned documents and re-assign them (Move from a templ location to another location))
                    reassignRequestDocumentsToCase(repository, assignDocs);
                    WriteOutput("Assigned " + Math.Ceiling(assignDocs.Count * .30) + " in " + ((DateTime.Now.Ticks - tStart) / TimeSpan.TicksPerMillisecond) + "ms");
                    WriteOutput("-----END " + testName + "--------------");
                }
                catch (Exception e)
                {
                    WriteOutput("#####FAILED##### TEST [" + testName + "]" + e.StackTrace.ToString());
                }

                try
                {
                    testName = "GetDocumentForView";
                    WriteOutput("-----BEGIN " + testName + "-----------------");
                    tStart = DateTime.Now.Ticks;
                    for (int p = 0; p < (Math.Ceiling(assignDocs.Count * .3)); p++)
                    {
                        ViewDocument(repository, primaryContentDirectory, assignDocs[p].DocumentId, openEachFile);
                    }
                    if (showdownloadedfiles) System.Diagnostics.Process.Start(primaryContentDirectory);
                    WriteOutput("Re-Assigned " + Math.Ceiling(assignDocs.Count * .30) + " in " + ((DateTime.Now.Ticks - tStart) / TimeSpan.TicksPerMillisecond) + "ms");
                    WriteOutput("-----END " + testName + "-------------------");
                }
                catch (Exception e)
                {
                    WriteOutput("#####FAILED##### TEST [" + testName + "]" + e.StackTrace.ToString());
                }

                try
                {
                    testName = "GetDocumentHistory";
                    WriteOutput("-----BEGIN " + testName + "-----------------");
                    tStart = DateTime.Now.Ticks;
                    checkDocumentHistory(repository, assignDocs);
                    WriteOutput("Fetched RestDocument History of  " + Math.Ceiling(assignDocs.Count * .10) + " Documents in " + ((DateTime.Now.Ticks - tStart) / TimeSpan.TicksPerMillisecond) + "ms");
                    WriteOutput("-----END " + testName + "-------------------");
                }
                catch (Exception e)
                {
                    WriteOutput("#####FAILED##### TEST [" + testName + "]" + e.StackTrace.ToString());
                }

                try
                {
                    testName = "CreateRendition";
                    WriteOutput("-----BEGIN " + testName + "--------------------");
                    tStart = DateTime.Now.Ticks;
                    idsWithRenditions = CreateRendition(repository, assignDocs, 0, false);
                    WriteOutput("Imported new Renditions of  " + Math.Ceiling(assignDocs.Count * .30) + " Documents in " + ((DateTime.Now.Ticks - tStart) / TimeSpan.TicksPerMillisecond) + "ms");
                    WriteOutput("-----END " + testName + "----------------------");
                }
                catch (Exception e)
                {
                    WriteOutput("#####FAILED##### TEST [" + testName + "]" + e.StackTrace.ToString());
                }

                try
                {
                    testName = "ViewRenditions";
                    WriteOutput("-----BEGIN " + testName + "--------------------");
                    tStart = DateTime.Now.Ticks;
                    // This may be used to get the text version of the email for the correspondence view
                    ViewRenditions(repository, renditionsDirectory, idsWithRenditions, openEachFile);
                    // Open a directory with the downloaded renditions to show the tester
                    if (showdownloadedfiles) System.Diagnostics.Process.Start(renditionsDirectory);
                    WriteOutput("Renditions of  " + Math.Ceiling(assignDocs.Count * .30) + " Documents for view in " + ((DateTime.Now.Ticks - tStart) / TimeSpan.TicksPerMillisecond) + "ms");
                    WriteOutput("-----END " + testName + "----------------------");
                }
                catch (Exception e)
                {
                    WriteOutput("#####FAILED##### TEST [" + testName + "]" + e.StackTrace.ToString());
                }

                try
                {
                    testName = "ImportAsNewVersion";
                    WriteOutput("-----BEGIN " + testName + "----------------");
                    tStart = DateTime.Now.Ticks;
                    ImportAsNewVersion(repository, assignDocs);
                    WriteOutput("New Versions of  " + Math.Ceiling(assignDocs.Count * .20) + " Documents for created in " + ((DateTime.Now.Ticks - tStart) / TimeSpan.TicksPerMillisecond) + "ms");
                    WriteOutput("-----END " + testName + "------------------");
                }
                catch (Exception e)
                {
                    WriteOutput("#####FAILED##### TEST [" + testName + "]" + e.StackTrace.ToString());
                }

                try
                {
                    testName = "ReturnListOfDocuments";
                    WriteOutput("-----BEGIN " + testName + "----------------");
                    tStart = DateTime.Now.Ticks;
                    ReturnListOfDocuments(repository, assignDocs);
                    WriteOutput("List of Documents for 5 supportingDocs returned in " + ((DateTime.Now.Ticks - tStart) / TimeSpan.TicksPerMillisecond) + "ms");
                    WriteOutput("-----END " + testName + "------------------");
                }
                catch (Exception e)
                {
                    WriteOutput("#####FAILED##### TEST [" + testName + "]" + e.StackTrace.ToString());
                }
/**
 * The below items were enablements in DCTM-ReST to handle email import like WDK does (split email from attachments)
 * and a record management function to allow setting a event condition date. They are not in standard ReST Services
 */
//                try
//                {
//                    testName = "ImportEmail";
//                    WriteOutput("-----BEGIN " + testName + "----------------");
//                    tStart = DateTime.Now.Ticks;
//                    float pctTest = 1.0F;
//                    ImportEmail(repository, pctTest);
//                    WriteOutput("Imported " + (Math.Ceiling(
//                        (new DirectoryInfo(randomEmailsDirectory).GetFiles().Count()) * pctTest))
//                        + " emails in " + ((DateTime.Now.Ticks - tStart) / TimeSpan.TicksPerMillisecond) + "ms");
//                    WriteOutput("-----END " + testName + "------------------");
//                }
//                catch (Exception e)
//                {
//                    WriteOutput("#####FAILED##### TEST [" + testName + "]" + e.StackTrace.ToString());
//                }
//
//                try
//                {
//                    testName = "Close Case or Request";
//                    WriteOutput("-----BEGIN " + testName + "----------------");
//                    tStart = DateTime.Now.Ticks;
//                    CloseSupportingDocThenCase(repository);
//                    WriteOutput("Closed " + requestList.Count + " cases/supportingDocs in " + ((DateTime.Now.Ticks - tStart) / TimeSpan.TicksPerMillisecond) + "ms");
//                    WriteOutput("-----END " + testName + "------------------");
//                }
//                catch (Exception e)
//                {
//                    WriteOutput("#####FAILED##### TEST [" + testName + "]" + e.StackTrace.ToString());
//                }

//                try
//                {
//                    testName = "Re-Open Case or Request";
//                    WriteOutput("-----BEGIN " + testName + "----------------");
//                    tStart = DateTime.Now.Ticks;
//                    ReOpenCaseOrRequest(repository);
//                    WriteOutput("Closed " + requestList.Count + " cases/supportingDocs in " + ((DateTime.Now.Ticks - tStart) / TimeSpan.TicksPerMillisecond) + "ms");
//                    WriteOutput("-----END " + testName + "------------------");
//                }
//                catch (Exception e)
//                {
//                    WriteOutput("#####FAILED##### TEST [" + testName + "]" + e.StackTrace.ToString());
//                }
			double restVersion = Double.Parse(productInfo.Properties.Major);
			if (restVersion >= 7.2d) {
				try {
					testName = "SEARCH";
					WriteOutput ("---------------BEGIN " + testName + " ----------------------");
					SearchForDocuments (repository);
					WriteOutput ("-----------------END " + testName + " ----------------------");
				} catch (Exception e) {
					WriteOutput ("#####FAILED##### TEST [" + testName + "]" + e.StackTrace.ToString ());
				}
			}
				// These features do not work on Mono yet, should be fine when .NetCore is released though
			if(Type.GetType ("Mono.Runtime") == null) {
	                try
	                {
	                    testName = "ExportCase";
	                    WriteOutput("---------------BEGIN " + testName + " ------------------");
	                    ExportCase(repository);
	                    WriteOutput("-----------------END " + testName + " ------------------");
	                }
	                catch (Exception e)
	                {
	                    WriteOutput("#####FAILED##### TEST [" + testName + "]" + e.StackTrace.ToString());
	                }

	                try
	                {
	                    testName = "ExportListOfFiles";
	                    WriteOutput("---------------BEGIN " + testName + " ------------------");
	                    ExportFiles(repository);
	                    WriteOutput("-----------------END " + testName + " ------------------");
	                }
	                catch (Exception e)
	                {
	                    WriteOutput("#####FAILED##### TEST [" + testName + "]" + e.StackTrace.ToString());
	                }
			} // END IF (Mono) exclusion
                WriteOutput("#####################################");
                WriteOutput("COMPLETED TESTS IN: " + ((DateTime.Now.Ticks - testStart) / TimeSpan.TicksPerMillisecond) / 1000 / 60 + "minutes");
                WriteOutput("#####################################");
                System.Diagnostics.Process.Start(testDirectory);

                if (loggerForm != null)
                {
                    while (loggerForm.Visible)
                    {
                        Application.DoEvents();
                    }
                }
        }

        private void ExportFiles(Repository repository)
        {
            FeedGetOptions options = new FeedGetOptions { ItemsPerPage = 10 };
            Feed<RestDocument> feedDocs = repository.ExecuteDQL<RestDocument>("select r_object_id from dm_document where folder('" + ProcessBasePath + caseId + "', DESCEND)", options);
            List<RestDocument> docs = ObjectUtil.getFeedAsList<RestDocument>(feedDocs, true);
            StringBuilder ids = new StringBuilder();
            int pass = 0;
            foreach(RestDocument doc in docs) {
                string id = doc.getAttributeValue("r_object_id").ToString();
                if (pass == 0)
                {
                    pass++;
                    ids.Append(id);
                } else {
                    ids.Append(",").Append(id);
                }
            }
            WriteOutput("[ExportFilesToZip] - Export list of files completed and stored: " + testDirectory + Path.DirectorySeparatorChar + "RandomDocsInCase.zip");
            repository.ExportDocuments(ids.ToString(), testDirectory + Path.DirectorySeparatorChar + "RandomDocsInCase.zip");
        }

        private void ExportCase(Repository repository)
        {
            string casePath = ProcessBasePath + caseId;
			FileInfo zipFile = repository.ExportFolder(casePath, testDirectory + Path.DirectorySeparatorChar + caseId + ".zip");
			WriteOutput("[ExportFolderToZip] Export Folder completed and stored: " + testDirectory + Path.DirectorySeparatorChar + caseId + ".zip");
        }

        private void CreateFromTemplate(Repository repository, List<AssignDocument> assignDocs)
        {
            
            Random rnd = new Random();
            //get list of templates
            Feed<RestDocument> results = repository.ExecuteDQL<RestDocument>(String.Format("select * from dm_document where FOLDER('/Templates') "), new FeedGetOptions { Inline = true, Links = true });
            List<RestDocument> docs = ObjectUtil.getFeedAsList<RestDocument>(results, true);
            int resultSamples = docs.Count;

            WriteOutput(String.Format("\t[TemplateList] Returning list of templates..."));
            foreach (RestDocument doc in docs)
            {
                WriteOutput(String.Format("\t\t\tTemplate Name: {0} ID: {1}",
                        doc.getAttributeValue("object_name").ToString(),
                        doc.getAttributeValue("r_object_id").ToString()));
            }
            List<string> req = requestList;
            for (int i = 0; i < 10; i++)
            {
                AssignDocument assignDoc = assignDocs[rnd.Next(0, assignDocs.Count)];
                string assignPath = assignDoc.getPath();
                string requestId = assignDoc.RequestId;
                //select one of the documents
                RestDocument template = docs[rnd.Next(0, resultSamples)];
                RestDocument newDoc = repository.copyDocument(template.getAttributeValue("r_object_id").ToString(), ProcessBasePath + assignPath);

                newDoc.setAttributeValue("subject", "Created From Template: " + template.getAttributeValue("object_name"));
                string documentName = ObjectUtil.NewRandomDocumentName("FROMTEMPLATE");
                newDoc.setAttributeValue("object_name", documentName);
                newDoc.Save();
                string objectId = newDoc.getAttributeValue("r_object_id").ToString();
                //String requestId = caseId + " REQUEST-" + new Random().Next(0, 5);
                assignDocs.Add(new AssignDocument(objectId, caseId, requestId));
                WriteOutput("\t[CreateDocumentFromTemplate] Created document " + documentName + " from template " + template.getAttributeValue("object_name").ToString());
            }
        }

        public void SearchForDocuments(Repository repository)
        {
            long tStart = DateTime.Now.Ticks;

            //WriteOutput("Pausing for ten seconds to make sure indexer has caught up for full text search test....");
            //Thread.Sleep(10000);
            Search search = new Search();
            search.Query = "document";
            search.Locations = "/SystemA/Process/" + caseId; // requestList[new Random().Next(0, requestList.Count)];

            search.ItemsPerPage = 20;
            //search.PageNumber = 1;
            int totalResults = 0;
            double totalPages = 0d;
            WriteOutput("[SearchResults] Return a list of documents using search criteria: " + search.Query + " Location: '" + search.Locations + "'");
            SearchFeed<RestDocument> feedResults = repository.ExecuteSearch<RestDocument>(search);
            if (feedResults != null)
            {
                totalResults = feedResults.Total;
                totalPages = feedResults.PageCount;
                int docProcessed = 0;

                for (int i = 0; i < totalPages; i++)
                {
                    foreach (SearchEntry<RestDocument> result in feedResults.Entries)
                    {
                        WriteOutput("\t[SearchResults] Search - RestDocument: " + result.Content.getAttributeValue("object_name").ToString() + " Summary: " + result.Summary
                            + " Score: " + result.Score + " Terms: " + String.Join(",", result.Terms));
                        docProcessed++;
                    }

                    if (totalResults > docProcessed) feedResults = feedResults.NextPage();
                    WriteOutput("\t*****************************************************");
                    WriteOutput("Page:" + (i + 1) + " Results: " + docProcessed + " out of " + totalResults + " Processed");
                    WriteOutput("\t*****************************************************");
                    WriteOutput("\n\n");
                }

            }
            WriteOutput("[SearchResults] Result Count: " + totalResults + " Pages: " + totalPages + " Processed in " + ((DateTime.Now.Ticks - tStart) / TimeSpan.TicksPerMillisecond) + "ms");
        }

        public void ImportEmail(Repository repository, float pctTest)
        {
            Random rnd = new Random();
            DirectoryInfo dirInfo = new DirectoryInfo(randomEmailsDirectory);
            int fileCount = dirInfo.GetFiles().Count();
            for (int i = 0; i < (Math.Ceiling(fileCount * pctTest * 10)); i++)
            {
                string assignPath = requestList[rnd.Next(0, requestList.Count)];
                FileInfo file = ObjectUtil.getRandomFileFromDirectoryByExtension(randomEmailsDirectory, "msg");
                if (file == null)
                {
                    WriteOutput("####EMAILIMPORTFAIL##### - No MSG Files are in your test files directory" + randomEmailsDirectory + " unable to do mailimport test");
                    return;
                }
                EmailPackage email = repository.ImportEmail(file, testPrefix + "-" + file.Name, assignPath);
                WriteOutput("\t[EmailImport] Email Import\t[DeDuplication] Email-De-duplication");
                Boolean isDuplicate = email.IsDuplicate;
                if (isDuplicate)
                {
                    WriteOutput("\t\t" + email.Email.getAttributeValue("object_name") + " is a DUPLICATE");
                    if (email.DuplicateType == DuplicateType.FOLDER)
                    {
                        WriteOutput("\t\t" + email.Email.getAttributeValue("object_name") + " already exists in " + assignPath + " object returned is the pre-existing email object.");
                    }
                    else
                    {
                        WriteOutput("\t\t" + email.Email.getAttributeValue("object_name") + " already exists elsewhere, LINKING to " + assignPath);
                        repository.linkToFolder(email.Email, assignPath);
                    }
                }
                else
                {
                    WriteOutput("\t\t" + email.Email.getAttributeValue("object_name") + "was imported");
                }
                
                foreach(RestDocument att in email.Attachments) 
                {
                    WriteOutput("\t\t\t Attachment:" + att.getAttributeValue("object_name"));
                }
            } 
        }

        private void CloseSupportingDocThenCase(Repository repository)
        {
            WriteOutput("\t[DeclareRecord,SetCloseCondition,UnsetCloseCondition]");
            foreach (string cr in requestList)
            {
                Folder requestFolder = null;
                requestFolder = repository.getFolderByPath(cr);
                DateTime closeDate = DateTime.Now;
                List<PersistentObject> retainers = repository.CloseCaseOrRequest(Repository.RecordType.Extradition, requestFolder, closeDate);
                DateTime checkDate = DateTime.Parse(retainers[0].getAttributeValue("event_date").ToString());
                try
                {

                    if (checkDate.ToShortDateString().Equals(closeDate.ToShortDateString()))
                    {
                        WriteOutput("\t\t[SetCloseCondition] - Closing REQUEST" + requestFolder.getAttributeValue("object_name") + " Declared as: " + Repository.RecordType.Extradition);
                    }
                    else throw new Exception();
                }
                catch (Exception e)
                {
                    WriteOutput("\t\t#####FAILED!!E####[SetCloseCondition] - Closing REQUEST" + requestFolder.getAttributeValue("object_name") + "  as: " + Repository.RecordType.Extradition);
                }
                
            }
            Folder caseFolder   = repository.getFolderByPath(ProcessBasePath + caseId);
            repository.CloseCaseOrRequest(Repository.RecordType.MLAT, caseFolder, DateTime.Now);
            WriteOutput("\t\t[SetCloseCondition] - Closing CASE " + caseFolder.getAttributeValue("object_name") + " Declared as: " + Repository.RecordType.MLAT);
                
        }

        private void ReOpenCaseOrRequest(Repository repository)
        {
            
            for (int i = 0; i < (Math.Ceiling(requestList.Count * .20)); i++)
            {
                string cr = requestList[i];
                Repository.RecordType type = Repository.RecordType.MLAT;
                Folder folder = null;
                if (cr.Contains("/"))
                {
                    type = Repository.RecordType.Extradition;
                    folder = repository.getFolderByPath(cr);
                }
                else
                {
                    folder = repository.getFolderByPath(ProcessBasePath + cr);
                }
                // Pass a null date to zero out the Close Date event on the retainer to stop aging
                List<PersistentObject> retainers = repository.CloseCaseOrRequest(type, folder, new DateTime());


                if (retainers[0].getAttributeValue("event_date") == null)
                {
                    WriteOutput("\t[UnsetCloseCondition] " + folder.getAttributeValue("object_name"));        
                }
                else
                {
                    WriteOutput("\t#####FAILED#####!! [UnsetCloseCondition] " + folder.getAttributeValue("object_name"));        
                }
            }
        }

        private void ReturnListOfDocuments(Repository repository, List<AssignDocument> assignDocs)
        {
            Random rnd = new Random();
            // Return a list of documents in a case
            int resultSamples = assignDocs.Count < 5? assignDocs.Count : 5;
            for(int i=0; i<resultSamples;i++) {
                string path = ProcessBasePath + assignDocs[rnd.Next(0,resultSamples)];
                Feed<RestDocument> results = repository.ExecuteDQL<RestDocument>(
                    String.Format("select * from dm_document where folder('{0}', DESCEND)", path), new FeedGetOptions { Inline=true, Links=true });
                List<RestDocument> docs = ObjectUtil.getFeedAsList<RestDocument>(results, true);
                WriteOutput(String.Format("\t\t[ReturnListOfDocumentsFromQuery] Returning list of documents in path [{0}]", path));
                foreach (RestDocument doc in docs)
                {
                    WriteOutput(String.Format("\t\t\tName: {0} ID: {1}", 
                        doc.getAttributeValue("object_name").ToString(), 
                        doc.getAttributeValue("r_object_id").ToString()));
                }
            }
            
        }

        private void ImportAsNewVersion(Repository repository, List<AssignDocument> assignDocs)
        {
            List<AssignDocument> myList = new List<AssignDocument>(assignDocs);
            WriteOutput(("\t[ImportAsNewVersion] Importing new Content to existing objects as New Versions"));
            for (int i = 0; i < (Math.Ceiling(myList.Count * .20)); i++)
            {
                AssignDocument aDoc = myList[i];
                myList.Remove(aDoc);

                RestDocument doc = repository.getObjectById<RestDocument>(myList[i].DocumentId); 
                //RestDocument doc = repository.getDocumentByQualification(String.Format("dm_document where r_object_id = '{0}'",
                //    myList[i].DocumentId), new FeedGetOptions { Inline = true, Links = true });
                Feed<OutlineAtomContent> versions = doc.GetVersionHistory<OutlineAtomContent>(null);
                List <Entry<OutlineAtomContent>> entries = versions.Entries;
                WriteOutput("\t\tCurrentDocumentVersion: " + doc.getRepeatingValuesAsString("r_version_label", ",") + " ID: " + doc.getAttributeValue("r_object_id").ToString());
                WriteOutput("\t\tVersion Count Prior to Importing New Version:" + entries.Count);
                doc = doc.Checkout();
                if (doc.IsCheckedOut())
                {
                    WriteOutput("\t\t[CheckOut] - Checked out document...");
                    doc = doc.CancelCheckout();
                    if(!doc.IsCheckedOut()) {
                        WriteOutput("\t\t[CancelCheckOut] - Canceled Checkout....");
                        doc = doc.Checkout();
                        if (doc.IsCheckedOut())
                        {
                            WriteOutput("\t\t[CheckOut] - Checked out document after cancel checkout..., document is checked out by: " +doc.getLockOwner());        
                        } else {
                            WriteOutput("\t\t[CheckOut] - #####FAILED##### CHECK OUT DOCUMENT");
                        }
                    }
                }
                else
                {
                    WriteOutput("\t\t[CheckOut] - #####FAILED##### CHECK OUT DOCUMENT");
                }
                FileInfo file = ObjectUtil.getRandomFileFromDirectory(randomFilesDirectory);
                doc = repository.ImportDocumentAsNewVersion(doc, file);
                WriteOutput("\t [ImportAsNewVersion] Import RestDocument as New Version");

                if (doc.IsCheckedOut())
                {
                    WriteOutput("\t\t[CheckIn] - #####FAILED##### CHECK IN DOCUMENT");
                }
                else
                {
                    WriteOutput("\t\t[CheckIn] - RestDocument Checked IN...");
                }

                Feed<OutlineAtomContent> newVersions = doc.GetVersionHistory<OutlineAtomContent>(null);
                List <Entry<OutlineAtomContent>> newEntries = newVersions.Entries;
                WriteOutput("\t\tNew Version Count: " + newEntries.Count);
                WriteOutput("\t\tNewDocumentVersion: " + doc.getRepeatingValuesAsString("r_version_label", ",") + " ID: " + doc.getAttributeValue("r_object_id").ToString());
                WriteOutput("\t\t[ListVersions] - List of document versions:");
                WriteOutput("Versions:");
                List<RestDocument> allVersions = repository.getAllDocumentVersions(doc);
                foreach (RestDocument vDoc in allVersions)
                {
                    WriteOutput(String.Format("\t\t\t ChronicleID: {0} ObjectID: {1} VersionLabel: {2}",
                        doc.getAttributeValue("i_chronicle_id").ToString(),
                        vDoc.getAttributeValue("r_object_id").ToString(),
                        vDoc.getRepeatingValuesAsString("r_version_label", ",")));
                }
            }
            
        }

        public void ViewDocument(Repository repository, String path, string objectId, bool openDocument)
        {

            RestDocument doc = repository.getObjectById<RestDocument>(objectId);

            ContentMeta contentMeta = doc.getContent();
            if (contentMeta == null)
            {
                WriteOutput("!!!!!!!!!!!!!!!!VIEW TEST FAILURE!!!!!!!!!!!!!!!!!!!!");
                return;
            }
            FileInfo downloadedContentFile = contentMeta.DownloadContentMedia();
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
			downloadedContentFile.MoveTo(path + Path.DirectorySeparatorChar + objectId + "-" + downloadedContentFile.Name);
            WriteOutput("\t\t[GetFileForView] - RestDocument file is located: " + downloadedContentFile.FullName);
            if (openDocument) System.Diagnostics.Process.Start(downloadedContentFile.FullName);
        }

        private void checkDocumentHistory(Repository repository, List<AssignDocument> assignDocs)
        {
            List<AssignDocument> newList = new List<AssignDocument>(assignDocs);

            double assignCount = Math.Ceiling(newList.Count * .10);
            for (int a = 0; a < assignCount; a++)
            {
                AssignDocument aDoc = ObjectUtil.getRandomObjectFromList<AssignDocument>(newList);
                // Make sure we do not use this again
                newList.Remove(aDoc);
                RestDocument doc = repository.getObjectById<RestDocument>(aDoc.DocumentId); //repository.getDocumentByQualification(
                    //String.Format("dm_document(all) where r_object_id = '{0}'",aDoc.DocumentId), null);
                WriteOutput("\t" + aDoc.DocumentId + ":" + doc.getAttributeValue("object_name").ToString() + " - RestDocument History:");
                Feed<AuditEntry> auditInfo = repository.getDocumentHistory(HistoryType.THISDOCUMENTONLY, doc);
                List<AuditEntry> entries = ObjectUtil.getFeedAsList(auditInfo,true);
                //List<Entry<AuditEntry>> entries = (List<Entry<AuditEntry>>)auditInfo.Entries;
                foreach (AuditEntry history in entries)
                {
                    WriteOutput("\t\tEvent:" + history.getEventName() + " Description:" + history.getEventDescription()
                        + " ObjectName:" + history.getObjectName() + " Time:" + history.getTimeStamp());
                }
                WriteOutput("\t\t[AuditHistory] - RestDocument history pulled from the Audit tables");
            }
        }

        /// <summary>
        /// Move 5 percent of the (no less than 1) documents from request to case level (Shows Re-Filing Documents)
        /// </summary>
        /// <param name="assignDocs"></param>
        /// <param name="repository"></param>
        private void reassignRequestDocumentsToCase(Repository repository, List<AssignDocument> assignDocs)
        {
            List<AssignDocument> newList = new List<AssignDocument>(assignDocs);
            double assignCount = Math.Ceiling(newList.Count * .30);
            for (int a = 0; a < assignCount; a++)
            {
                AssignDocument aDoc = ObjectUtil.getRandomObjectFromList<AssignDocument>(newList);
                // Make sure we do not use this again
                newList.Remove(aDoc);
                String currentPath = ProcessBasePath + aDoc.getPath();
                RestDocument docToMove = repository.getObjectById<RestDocument>(aDoc.DocumentId);//getDocumentByQualification(
                    //String.Format("dm_document where r_object_id = '{0}'", aDoc.DocumentId),
                    //new FeedGetOptions { Inline = true, Links = true });
                List<String> requestPathAndFolder = ObjectUtil.getPathAndFolderNameFromPath(currentPath);
                String caseFolderPath = requestPathAndFolder[0];
                String requestFolderName = requestPathAndFolder[1];
                Folder requestFolder = repository.getFolderByQualification(
                    String.Format("dm_folder where folder('{0}') and object_name='{1}'", caseFolderPath,
                    requestFolderName), new FeedGetOptions { Inline = true, Links = true });
                List<String> casePathAndFolder = ObjectUtil.getPathAndFolderNameFromPath(caseFolderPath);
                String caseParentFolder = casePathAndFolder[0];
                String caseFolderName = casePathAndFolder[1];
                Folder caseFolder = repository.getFolderByQualification(
                    String.Format("dm_folder where folder('{0}') and object_name='{1}'", caseParentFolder,
                    caseFolderName), new FeedGetOptions { Inline = true, Links = true });
                repository.moveDocument(docToMove, requestFolder, caseFolder);
                WriteOutput("\t\t[MoveDocument] - RestDocument reassigned from " + currentPath + " to " + caseFolderPath);
            }
        }

        /// <summary>
        /// CreateTempDocs and AssignDocs, together, are the first use case from the Instantiation Wizard Form of Process
        /// Encompases  [Import New RestDocument], [Copy/Move RestDocument], [DeleteDocument]
        ///             [Folder Structure]
        /// </summary>
        /// <param name="repository"></param>
        /// <returns></returns>
        public List<AssignDocument> CreateTempDocs(Repository repository)
        {
            List<AssignDocument> assignDocs = new List<AssignDocument>();
            tempPath = tempPath + "-" + threadNum;

            WriteOutput("[CreateOrGetFolderPath] - Creating or getting folder by path: " + tempPath);
            Folder tempFolder = repository.getOrCreateFolderByPath(tempPath);
            WriteOutput("\tFolder: " + tempFolder.getAttributeValue("object_name") + ":" 
                + tempFolder.getAttributeValue("r_object_id") + " successfully created!");
            WriteOutput("\tCreating " + numDocs + " random documents.");
            string previousRequestId = null;
            for (int i = 0; i < numDocs; i++)
            {
                FileInfo file = ObjectUtil.getRandomFileFromDirectory(randomFilesDirectory);
                
                RestDocument tempDoc = repository.ImportNewDocument(file, testPrefix + "-" + file.Name, tempPath);
                WriteOutput("\t\t[ImportDocument] - RestDocument " + file.FullName + " imported as " 
                    + tempDoc.getAttributeValue("object_name") + " ObjectID: " 
                    + tempDoc.getAttributeValue("r_object_id").ToString());
                WriteOutput("\t\t\t[DeDuplication] - Performing Duplicate Detection on content in holding area....");
                CheckDuplicates(repository, tempDoc, tempPath);

                // Cannot randomly assign cases as threads will step on each other. Limite one thread to one 
                // case (which should be reflective of the actual use case).
                
                String requestId = caseId + " REQUEST-" + new Random().Next(0,5);
                String objectId = (String)tempDoc.getAttributeValue("r_object_id");
                WriteOutput("[CreateAndAssignDocument] \t\tCreated " + tempDoc.getAttributeValue("object_name") + ":" + objectId + " Assigning to Case: " 
                    + caseId + " Request: " + requestId);
                WriteOutput("[ChangeExistingDocument] - ReFetching and Setting title attribute");
                RestDocument doc = tempDoc.fetch<RestDocument>();
                doc.setAttributeValue("title", "Set properties test");
                doc.Save();
                assignDocs.Add(new AssignDocument(objectId, caseId, requestId));
                // To show we can assign the same document to multiple supportingDocs
                if (previousRequestId != null && !previousRequestId.Equals(requestId))
                {
                    WriteOutput("\t\t\tAssigning this document to another request to show the same document can be copied/assigned to multiple supportingDocs");
                    assignDocs.Add(new AssignDocument(objectId, caseId, previousRequestId));
                }
                previousRequestId = requestId;
            } // Done with temp creation loop
            return assignDocs;
        }

        private void CheckDuplicates(Repository repository, RestDocument doc, string path)
        {
            List<PersistentObject> dupes = repository.CheckForDuplicate((String)doc.getAttributeValue("r_object_id"), path);
            StringBuilder dupeList = new StringBuilder();
            if (dupes.Count != 0)
            {
                if (printResult)
                {
                    bool first = true;
                    WriteOutput("\t\t\tDocument: " + doc.getAttributeValue("object_name") + ":" + doc.getAttributeValue("r_object_id"));
                    foreach (PersistentObject pObj in dupes)
                    {
                        WriteOutput(String.Format("DUPLICATE OF: {0}", pObj.getRepeatingValuesAsString("parent_id",",").ToString()));
                        if (first)
                        {
                            dupeList.Append("'" + pObj.getAttributeValue("parent_id") + "'");
                        }
                        else
                        {
                            dupeList.Append(",'" + pObj.getAttributeValue("parent_id") + "'");
                        }
                    }
                    
                }

                if (path == null)
                {
                    WriteOutput("\t\t\t[DeDuplication] - " + dupes.Count + " duplicates were identified in the SYSTEM. Choosing to allow");
                }
                else
                {
                    WriteOutput("\t\t\t[DeDuplication] - " + dupes.Count + " duplicates were identified in the destination FOLDER, Choosing to allow.");
                }

            }
            else
            {
                WriteOutput("\t\t\t[DeDuplication] - No Duplicates of this document found.");
            }
        }

        /// <summary>
        /// Called by CreateTempDocs in order to create case/supportingDocs and randomly assign documents to the cases.
        /// </summary>
        /// <param name="repository"></param>
        /// <param name="assignDocs"></param>
        private void AssignToCaseSupportingDoc (Repository repository, List<AssignDocument> assignDocs)
        {
            //WriteOutput("Getting the holding folder for documents prior to Case/Request assignment...");
            Folder tempFolder = repository.getFolderByQualification("dm_folder where any r_folder_path = '" 
                + tempPath + "'", new FeedGetOptions{ Inline = true, Links = true });
            WriteOutput("\tAssigning Documents from folder: " + tempFolder.getAttributeValue("object_name"));
            foreach (AssignDocument assignDoc in assignDocs)
            {
                String caseId = assignDoc.CaseId;
                String requestId = assignDoc.RequestId;
                //WriteOutput("Getting the Case/Request assignment folder...");
                String assignPath = ProcessBasePath + assignDoc.getPath();
                // Our case/request tracker for doing record declaration later
                addSupportingDoc(assignPath);
                Folder destinationDir = repository.getOrCreateFolderByPath(assignPath);
                RestDocument docToCopy = repository.getObjectById<RestDocument>(assignDoc.DocumentId); // getDocumentByQualification("dm_document where r_object_id = '"
                     //+ assignDoc.DocumentId + "'", new FeedGetOptions { Inline = true, Links = true });
                // To copy the document, we need to get a reference object
                CheckDuplicates(repository, docToCopy, ProcessBasePath + assignDoc.getPath());
                RestDocument copiedDoc = destinationDir.CreateSubObject<RestDocument>(docToCopy.GetCopy<RestDocument>(), null);
                WriteOutput("\t[CopyDocument] - Assigned RestDocument: " + copiedDoc.getAttributeValue("object_name") + " ID:" 
                    + assignDoc.DocumentId + " to " + ProcessBasePath + assignDoc.getPath());
                // Update the assignDocumentId to the newly copied document
                assignDoc.DocumentId = copiedDoc.getAttributeValue("r_object_id").ToString();
            }

            // Delete our temp folder
            repository.deleteFolder(tempFolder, true, true);
            WriteOutput("[DeleteFolderAndContents] - Deleted the holding folder and documents");
        }

        public List<string> CreateRendition(Repository repository, List<AssignDocument> assignDocs, int page, bool isPrimary)
        {
            List<string> idsWithRenditions = new List<string>();

            List<AssignDocument> newList = new List<AssignDocument>(assignDocs);

            double assignCount = Math.Ceiling(newList.Count * .30);
            for (int a = 0; a < assignCount; a++)
            {
                AssignDocument aDoc = ObjectUtil.getRandomObjectFromList<AssignDocument>(newList);
                // Make sure we do not use this again
                newList.Remove(aDoc);
                String objectId = aDoc.DocumentId;
                RestDocument doc = repository.getObjectById<RestDocument>(objectId); //getDocumentByQualification(
                //String.Format("dm_document where r_object_id = '{0}'", objectId), null);

                FileInfo file = ObjectUtil.getRandomFileFromDirectory(randomFilesDirectory);
                String mimeType = ObjectUtil.getMimeTypeFromFileName(file.Name);

                // Upload the content as a new rendition
                GenericOptions rendOptions = new GenericOptions();
                String format = ObjectUtil.getDocumentumFormatForFile(file.Extension);
                rendOptions.SetQuery("format", format);
                rendOptions.SetQuery("page", page);

                // If you want to allow multiple renditions of the same format, the modifier must be set, this makes the rendition unique in the list
                // the "modifier" is more like a label/tag for the rendition in the list.
                rendOptions.SetQuery("modifier", "Test");
                // With primary false, will be added as a rendition
                rendOptions.SetQuery("primary", isPrimary); 

                ContentMeta renditionMeta = doc.CreateContent(file.OpenRead(), mimeType, rendOptions);
                Feed<ContentMeta> contents = doc.GetContents<ContentMeta>(new FeedGetOptions { Inline = true });
                List<Entry<ContentMeta>> entries = (List<Entry<ContentMeta>>)contents.Entries;
                WriteOutput("\t\t[AddRendition] - Rendition Added for RestDocument ID: " 
                    + doc.getAttributeValue("r_object_id") + ":" 
                    + doc.getAttributeValue("object_name"));
                foreach (Entry<ContentMeta> entry in entries)
                {
                    ContentMeta rendition = entry.Content;
                    WriteOutput("\t\t\tRendition Format: " + rendition.getAttributeValue("full_format")
                        + " Modifier: " + rendition.getRepeatingString("page_modifier", 0)); //((Object[])rendition.getAttributeValue("page_modifier"))[0].ToString());
                }
                idsWithRenditions.Add(objectId);
            }
            return idsWithRenditions;
        }


        public void ViewRenditions(Repository repository, string renditionDirectory, List<string> idsWithRenditions, bool openDocument)
        {

            foreach (string objectId in idsWithRenditions)
            {
                RestDocument doc = repository.getObjectById<RestDocument>(objectId); //.getDocumentByQualification(
                    //String.Format("dm_document where r_object_id = '{0}'", objectId), null);

                ContentMeta renditionMeta = doc.getRenditionByModifier("Test");
                if (renditionMeta == null)
                {
                    WriteOutput("!!!!!!!!!!!!!!!!RENDITION VIEW TEST FAILURE!!!!!!!!!!!!!!!!!!!!");
                    return;
                }
                FileInfo downloadedContentFile = renditionMeta.DownloadContentMedia();
                if (!Directory.Exists(renditionDirectory))
                {
                    Directory.CreateDirectory(renditionDirectory);
                }
				downloadedContentFile.MoveTo(renditionDirectory + Path.DirectorySeparatorChar + objectId + "-" + downloadedContentFile.Name);
                WriteOutput("\t\t[ViewRendition] - Rendition file is located: " + downloadedContentFile.FullName);
                if(openDocument) System.Diagnostics.Process.Start(downloadedContentFile.FullName);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    loggerForm.Dispose();
                }
            }
            _disposed = true;
        }

        /// <summary>
        /// 
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
