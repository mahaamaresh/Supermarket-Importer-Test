using System;
using System.Threading;
using ImporterBLL.Properties;

namespace ImporterBLL.Objects
{
    public abstract class ErrorHandledService
    {
       // public event EventHandler<ExceptionThrownEventArgs> ExceptionThrown;
        //protected System.Diagnostics.EventLog eventLog1;

        public virtual void Start()
        {
            //eventLog1.WriteEntry("In Start");

            try
            {
                //eventLog1.WriteEntry("Before Logging " + ServiceSettings.Default.ServiceName);
           //     Log.Write(SeverityTypes.Information, "{0} Started By Service Manager", ServiceSettings.Default.ServiceName);
                Finish = false;
                //eventLog1.WriteEntry("After Logging");

                //eventLog1.WriteEntry("Before OnStart");
                OnStart();
                //eventLog1.WriteEntry("After OnStart");

                while (!Finish)
                {
                    //eventLog1.WriteEntry("Entering loop, Condition" + !Finish);

                    var outcome = Constants.ProcessOutcome.Success;
                    try
                    {
                      //  Log.Write(SeverityTypes.Verbose, "Calling Process() function from ErrorHandledService");
                        outcome = Process();
                    }
                    catch (Exception e) //If anything unforseen goes wrong log it
                    {
                       // Log.Write(SeverityTypes.Critical, e);
                        Thread.Sleep(Settings.Default.ErrorSleepPeriod);
                    }
                    finally
                    {
                     //   Log.Write(SeverityTypes.Verbose, "Returned from Process() function in ErrorHandledService");
                        if (outcome != Constants.ProcessOutcome.ForcedStopped && outcome != Constants.ProcessOutcome.AdhocSuccess) //if importer was force stopped, most likely because adhoc import is waiting to be run, go straight back to Process.
                        {
                     //       Log.Write(SeverityTypes.Verbose, string.Format("Sleeping for {0} in ErrorHandledService", ServiceSettings.Default.SleepPeriod));
                            Thread.Sleep(Settings.Default.SleepPeriod);
                        }
                    }
                }
            }
            catch (Exception )
            {

                //using(var file = new System.IO.StreamWriter("log.txt", true))
                //{
                //    file.WriteLine(e.Message + "\n" + e.StackTrace + "\n");
                //}

              //  Log.Write(SeverityTypes.Critical, e, "Error in OnStarting");
                Finish = true;//stop the service.
            }
            finally
            {
                try
                {
                    OnStop();
                }
                catch (Exception )
                {
                 //   Log.Write(SeverityTypes.Critical, e, "Error in OnStopping");
                }
            }

        }
                
        public virtual void Stop()
        {
         //   Log.Write(SeverityTypes.Information, "{0} stopped By Service Manager", ServiceSettings.Default.ServiceName);
            Finish = true;
        }

        protected virtual void OnStart()
        {
            Startup();
        }

        protected virtual void OnStop()
        {
            Cleanup();
        }
        
        [Obsolete("Please use OnStop override instead")]
        protected virtual void Cleanup(){}
        
        [Obsolete("Please use OnStart override instead")]
        protected virtual void Startup(){}

        protected abstract Constants.ProcessOutcome Process();

        private bool Finish { get; set; }
         /*
        [Obsolete]
        public void ManageError(ErrorLevel errorLevel, string errorType, string errorMessage, string errorDetails, bool handledGracefully)
        {
            
            Log.Write(GetSevTypeFromErrorType(errorLevel),
                "Type: {0}, Message: {1}, Details: {2}, Handled: {3}",
                errorType,
                errorMessage,
                errorDetails,
                handledGracefully);
            
        }

        [Obsolete]
        public void ManageException(Exception err, bool handledGracefully)
        {
            var args = new ExceptionThrownEventArgs(err, handledGracefully);
            // Call the events to give the hosting application a chance to handle the exception gracefully
            if (ExceptionThrown != null)
                ExceptionThrown(this, args);

          //  Log.Write(args.Handled ? SeverityTypes.Error : SeverityTypes.Critical, err, "Handled: {0}", args.Handled);
        }

       
        private SeverityTypes GetSevTypeFromErrorType(ErrorLevel level)
        {
            switch (level)
            { 
                case ErrorLevel.Anything :
                    return SeverityTypes.Information;
                case ErrorLevel.Error :
                case ErrorLevel.Warning :
                    return SeverityTypes.Warning;
                case ErrorLevel.Exception :
                    return SeverityTypes.Error;
                case ErrorLevel.Fatal :
                    return SeverityTypes.Critical;
                case ErrorLevel.Verbose :
                default :
                    return SeverityTypes.Verbose;
            }
        }
         * */
    }
}
