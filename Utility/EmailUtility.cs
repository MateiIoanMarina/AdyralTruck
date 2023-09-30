using Mailjet.Client.TransactionalEmails;
using Mailjet.Client;

namespace AdyralTruck.Utility
{
    public class EmailUtility
    {
        public static async Task SendEmail(
            string html, 
            string subject,
            string base64, 
            string fileName, 
            string contractBase64,
            string contractFileName,
            string emailAddress,
            bool contractDejaTrimis)
        {

            var attachments = new List<Attachment>();
            attachments.Add(new Attachment(fileName, "application/pdf", base64));

            if (!contractDejaTrimis)
            {
                attachments.Add(new Attachment(contractFileName, "application/pdf", contractBase64));
            }

            var email = new TransactionalEmailBuilder()
                .WithFrom(new SendContact("logistica.adyraltruck@gmail.com"))
                .WithSubject(subject)
                .WithHtmlPart($"{html}")
                .WithTo(new SendContact[] { new SendContact(emailAddress) })
                .WithAttachments(attachments)
                .WithBcc(new SendContact[] { new SendContact("logistica.adyraltruck@gmail.com"), new SendContact("i.marina.16@aberdeen.ac.uk") })
                .Build();

            var smsAccessToken1 = "89f306304848422b98cbfe7de86cc8bc";
            MailjetClient smsClient = new MailjetClient(smsAccessToken1);

            // invoke API to send email
            var response = await smsClient.SendTransactionalEmailAsync(email);
        }

        public static async Task SendEmailuri(
            string html, 
            string subject, 
            List<string> base64s, 
            string fileName, 
            string contractBase64, 
            string contractFileName, 
            string emailAddress,
            bool eContractDejaTrimis)
        {
            var attachments = new List<Attachment>();

            foreach(var base64 in base64s)
            {
                attachments.Add(new Attachment(fileName, "application/pdf", base64));
            }

            if (!eContractDejaTrimis)
            {
                attachments.Add(new Attachment(contractFileName, "application/pdf", contractBase64));
            }

            var email = new TransactionalEmailBuilder()
                .WithFrom(new SendContact("logistica.adyraltruck@gmail.com"))
                .WithSubject(subject)
                .WithHtmlPart($"{html}")
                .WithTo(new SendContact[] { new SendContact(emailAddress) })
                .WithAttachments(attachments)
                .WithBcc(new SendContact[] { new SendContact("logistica.adyraltruck@gmail.com"), new SendContact("i.marina.16@aberdeen.ac.uk") })
                .Build();

            var smsAccessToken1 = "89f306304848422b98cbfe7de86cc8bc";
            MailjetClient smsClient = new MailjetClient(smsAccessToken1);

            // invoke API to send email
            var response = await smsClient.SendTransactionalEmailAsync(email);
        }

        public static async Task SendDoarContractEmail(string html, string subject, string base64, string fileName, string emailAddress)
        {
            var email = new TransactionalEmailBuilder()
                .WithFrom(new SendContact("logistica.adyraltruck@gmail.com"))
                .WithSubject(subject)
                .WithHtmlPart($"{html}")
                .WithTo(new SendContact[] { new SendContact(emailAddress) })
                .WithAttachments(new Attachment[] { new Attachment(fileName, "application/pdf", base64) })
                .WithBcc(new SendContact[] { new SendContact("logistica.adyraltruck@gmail.com"), new SendContact("i.marina.16@aberdeen.ac.uk") })
                .Build();

            var smsAccessToken1 = "89f306304848422b98cbfe7de86cc8bc";
            MailjetClient smsClient = new MailjetClient(smsAccessToken1);

            // invoke API to send email
            var response = await smsClient.SendTransactionalEmailAsync(email);
        }
    }
}
