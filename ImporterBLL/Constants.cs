using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ImporterBLL
{
    public class Constants
    {
        public enum Services
        {
            ProductMaster = 1,
            ProductWebCategory = 2,
            ProductPriceRanging = 3,
            ProductLocation = 4,
            WeeklySpecials = 5,
            MultibuyOffers = 6,
            RelevantSpecials = 7,
            TargetedOffers = 8,
            Images = 9,
            ProductLocationDaily = 10,
            HealthWellbeing = 11
        }

        public enum ProcessOutcome
        {
            Success = 0,
            ExpectedFileNotFoundError = 1,
            TimeOfDayExceededError = 2,
            BulkInsertError = 3,
            UnzipError = 4,
            ValidatePropertyError = 5,
            UnhandledExceptionError = 6,
            HandledSQLExceptionError = 7,
            EndOfDayHasPassedError = 8,
            ArchiveError = 9,
            ForcedStopped = 10,
            AdhocSuccess = 11
        }

        public enum WaitReason
        {
            None = 0,
            FileDoesNotExist = 1
        }



    }
}


