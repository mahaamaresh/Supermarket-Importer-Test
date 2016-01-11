﻿using System;
using System.Collections.Generic;
using ImporterBLL.Helpers;
using ImporterBLL.Objects;
using WoolworthsDAL;

namespace ImporterBLL.Importers
{
    public class TargetedOfferPEL : ServiceImporterBase
    {
        private readonly string _fileName;
        private readonly string _apnMessage;
        private readonly bool _sendApns;
        private readonly bool _recordStats;

        public TargetedOfferPEL(string fileDirectoryPath, string archiveDirectoryPath, string stagingTableName, string formatFilePath, string fileName,
            string summaryReportErrorToEmailAddress, string summaryReportFromEmailAddress, string summaryReportFromAddressFriendlyName,  string apnMessage, bool sendApns, bool recordStats,
            string sqlaServerPath, string sqlbServerPath, string localSqlPath, string tempUploadFolder, string daysToRun)
            : base(ServiceLogger.FlatFileType.TargetedOffersPEL, FlatFiles.FileFormat.CSV, FlatFiles.CompressType.Zip,
            fileDirectoryPath, archiveDirectoryPath, stagingTableName, formatFilePath, summaryReportErrorToEmailAddress, summaryReportFromEmailAddress, summaryReportFromAddressFriendlyName,
            sqlaServerPath, sqlbServerPath,localSqlPath, tempUploadFolder, daysToRun)
        {
            _fileName = fileName;
            _apnMessage = apnMessage;
            _sendApns = sendApns;
            _recordStats = recordStats;
        }

        protected override List<string> FileNames
        {
            get
            {
                return new List<string>()
                {
                    { _fileName }
                };
            }
        }

        protected override bool ExecuteDataProcessingProc()
        {
            int success;

            using (var db = new WoolworthsDBDataContext())
            {
                db.CommandTimeout = CommandTimeoutInSeconds.Value;
                success = db.p_ImportTargetedOfferPEL(MasterLogId);
            }

            if (success == 0) return false;
            if (success == 1) return true;
            throw new ArgumentOutOfRangeException("success", "Stored Proc p_ImportTargetedOfferPEL returned int value that that was not equal to 1 or 0");
        }

        /// <summary>
        /// This method deals with the Apple Push Notifications that are relevant to this importer
        /// </summary>
        protected override void AfterDataProcessed()
        {
            /*
            if (_sendApns)
            {
                var distinctEdrNumbers = new List<string>();
                using (var db = new WoolworthsDBDataContext())
                {
                    db.CommandTimeout = CommandTimeoutInSeconds.Value;
                    distinctEdrNumbers = (from t in db.TargetedOffers
                                          where !t.ApnSent
                                          select t.EDRNo).Distinct().ToList();
                }

                Log(LogType.Log, String.Format("Attempting to send Apple Notifications for {0} distrinct Onecard numbers", distinctEdrNumbers.Count.ToString()));
                var customerHelper = new CustomerHelper();
                var apn = new ApplePush();

                string error;
                //int woolworthsGroupId = customerHelper.GetNewGroupIdForApns(distinctEdrNumbers, out error, isPel: false);

                //if (!String.IsNullOrEmpty(error))
                //{
                //    Log(LogType.Warning, String.Format("Aborting sending APNS - reason: {0}", error));
                //    return;
                //}

                var pelGroupId = customerHelper.GetNewGroupIdForApns(distinctEdrNumbers, out error, isPel: true);
                if (!String.IsNullOrEmpty(error))
                {
                    Log(LogType.Warning, String.Format("Aborting sending APNS - reason: {0}", error));
                    return;
                }

                //woolworths attempt
                //if (!apn.SendNotification(woolworthsGroupId, false, _apnMessage, _recordStats, out error))
                //{
                //    Log(LogType.Warning, String.Format("Aborting sending APNS - reason: {0}", error));
                //    return;
                //}

                //woolworths attempt
                if (!apn.SendNotification(pelGroupId, true, _apnMessage, _recordStats, out error))
                {
                    Log(LogType.Warning, String.Format("Aborting sending APNS - reason: {0}", error));
                    return;
                }

                var toLog = String.Format("notifications sent sucessfully");
                Log(LogType.Log, toLog);

                //update ApnLog table - Can't do this anymore... no way to get Certain success for individual EDRs
                if (_recordStats)
                {

                    var toInsert = new List<ApnLog>();

                    //using (WoolworthsDBDataContext db = new WoolworthsDBDataContext())//may cause timeout?
                    //{
                    //    db.CommandTimeout = CommandTimeoutInSeconds.Value;
                    //    db.ApnLogs.InsertAllOnSubmit(toInsert);
                    //    db.SubmitChanges();
                    //}
                }

                //update current rows in targeted offers table
                using (var db = new WoolworthsDBDataContext())
                {
                    db.CommandTimeout = CommandTimeoutInSeconds.Value;
                    db.p_ImportTargetedOfferPEL_UpdateAllApnSent();
                }
            }
             * */
        }

        protected override void ExecuteResetProc()
        {
            using (var db = new WoolworthsDBDataContext())
            {
                db.CommandTimeout = CommandTimeoutInSeconds.Value;
                db.p_ImportTargetedOfferPEL_Reset();
            }
        }

        protected override string DataProcessProcName
        {
            get { return "p_ImportTargetedOfferPEL"; }
        }

        protected override string ResetProcName
        {
            get { return "p_ImportTargetedOfferPEL_Reset"; }
        }
    }
}
