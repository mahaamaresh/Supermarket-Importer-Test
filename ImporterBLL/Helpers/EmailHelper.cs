using System;
using System.Collections.Generic;
using Amazon.EC2.Model;
using Amazon.SimpleEmail;
using Amazon.SimpleEmail.Model;
using ImporterBLL.Properties;

namespace ImporterBLL.Helpers
{
    public class EmailHelper
    {
        public static void SendException(Exception exception, string emailAddress, string subject, string body)
        {
            SendEmail(Settings.Default.SummaryReportFromEmailAddress, "", emailAddress, subject, body);
        }

        public static void SendEmail(string fromEmailAddress, string fromFriendlyName, string toEmailAddress, string emailSubject, string emailBody)
        {

/*

            var client = new AmazonSimpleEmailServiceClient(Settings.Default.AWSAccessKey, 
                                                           Settings.Default.AWSSecretKey,
                                                            Amazon.RegionEndpoint.EUWest1);
            
            var client = new AmazonSimpleEmailServiceClient("AKIAIJINFUWK57TVYABQ",
                                                           "AosdbK3YfN8zvFltHNDuxFZrHVZNlLeBZQ1pGKG8dFCd",
                                                            Amazon.RegionEndpoint.EUWest1);
            */
            /*
            var destination = new Destination(new List<string>() {toEmailAddress});
            var subjectContent = new Content(subject);
            var bodyContent = new Content(body);
            var messageBody = new Body(bodyContent);
            var message = new Message(subjectContent, messageBody);

            var request = new SendEmailRequest(fromEmailAddress, destination, message);
           
            client.SendEmail(request);
            */

            String FROM = fromEmailAddress;  // Replace with your "From" address. This address must be verified.
             String TO = toEmailAddress; // Replace with a "To" address. If you have not yet requested
            // production access, this address must be verified.

            const String SUBJECT = "Amazon SES test (AWS SDK for .NET)";
            const String BODY = "This email was sent through Amazon SES by using the AWS SDK for .NET.";

            // Construct an object to contain the recipient address.
            Destination destination = new Destination();
            destination.ToAddresses = (new List<string>() { TO });

            // Create the subject and body of the message.
            Content subject = new Content(SUBJECT);
            Content textBody = new Content(BODY);
            Body body = new Body(textBody);

            // Create a message with the specified subject and body.
            Message message = new Message(subject, body);

            // Assemble the email.
            SendEmailRequest request = new SendEmailRequest(FROM, destination, message);

            // Choose the AWS region of the Amazon SES endpoint you want to connect to. Note that your production 
            // access status, sending limits, and Amazon SES identity-related settings are specific to a given 
            // AWS region, so be sure to select an AWS region in which you set up Amazon SES. Here, we are using 
            // the US East (N. Virginia) region. Examples of other regions that Amazon SES supports are USWest2 
            // and EUWest1. For a complete list, see http://docs.aws.amazon.com/ses/latest/DeveloperGuide/regions.html 
            Amazon.RegionEndpoint REGION = Amazon.RegionEndpoint.EUWest1;

            // Instantiate an Amazon SES client, which will make the service call.
            var client = new AmazonSimpleEmailServiceClient(Settings.Default.AWSAccessKey,
                                                           Settings.Default.AWSSecretKey,
                                                            Amazon.RegionEndpoint.EUWest1);

            // Send the email.
          //  client.VerifyEmailAddress(new VerifyEmailAddressRequest() {EmailAddress = fromEmailAddress});


                client.SendEmail(request);
                Console.WriteLine("Email sent!");


        }
    }
}
