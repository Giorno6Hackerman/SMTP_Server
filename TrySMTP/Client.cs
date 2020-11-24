using System;
using System.Text;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Collections.Generic;
using System.Net.Mail;

namespace TrySMTP
{
    class Client
    {
        string mailbox = "D:/prog/4 sem/KSIS/Labs/Lab_4/mails";

        private List<string> Receiver = new List<string>();
        private string Sender;
        StringBuilder message = new StringBuilder();
        TcpClient client;

        public void Process()
        {
            bool flag = true;
            try
            {
                // Запрос клиента
                string Request = "";
                // Буфер для хранения принятых от клиента данных
                byte[] Buffer = new byte[1024];
                // Переменная для хранения количества байт, принятых от клиента
                int Count;
                // Ответ сервера

                while (true)
                {
                    try
                    {
                        Request = "";

                        if (flag)
                        {
                            flag = false;
                            SendResponse("220 OK\r\n");
                        }
                        else
                        {
                            Thread.Sleep(200);
                            while (client.GetStream().DataAvailable)
                            {
                                Count = client.GetStream().Read(Buffer, 0, Buffer.Length);
                                Request += Encoding.ASCII.GetString(Buffer, 0, Count);
                            }
                            Console.WriteLine(Request);

                            if (Request.IndexOf("\r\n.\r\n") != -1)
                            {
                                message.Append(Request);
                                SaveMassage(message.ToString());
                            }

                            DefineCommand(Request);
                        }
                    }
                    catch
                    {
                        break;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                if (client != null)
                    client.Close();
            }
        }

        public Client(TcpClient tcpClient)
        {
            client = tcpClient;
            
        }

        private void DefineCommand(string Request)
        {
            string command;
            if (Request.Length <= 0)
            {
                SendResponse("250 OK\r\n");
                return;
            }
            if (Request.IndexOf(" ") != -1)
            {
                command = Request.Substring(0, Request.IndexOf(" "));
            }
            else 
            {
                command = Request.Substring(0, Request.IndexOf("\r"));
            }
            switch (command)
            {
                case "EHLO":
                case "HELO":
                    SendResponse("250 mefistofel.lucifer.satan\r\n");
                    break;
                case "MAIL":
                    Sender = Request.Substring(Request.IndexOf("<") + 1, Request.IndexOf(">") - Request.IndexOf("<") - 1);
                    Receiver.Add(Sender);
                    SendResponse("250 OK\r\n");
                    break;
                case "RCPT":
                    Receiver.Add(Request.Substring(Request.IndexOf("<") + 1, Request.IndexOf(">") - Request.IndexOf("<") - 1));
                    SendResponse("250 OK\r\n");
                    /*if (Receiver.Contains(Request.Substring(Request.IndexOf("<") + 1, Request.IndexOf(">") - Request.IndexOf("<") - 1)))
                    {
                        SendResponse("250 OK\r\n");
                    }
                    else
                    {
                        SendResponse("550 No such user here");
                    }*/
                    break;
                case "DATA":
                    SendResponse("354 Start mail input; end with <CRLF>.<CRLF>");
                    break;
                case "RSET":
                    SendResponse("250 OK\r\n");
                    break;
                case "NOOP":
                    SendResponse("250 OK\r\n");
                    break;
                case "QUIT":
                    SendResponse("221 OK\r\n");
                    //SendBack();
                    client.Close();
                    //Console.WriteLine(message.ToString());
                    //Console.WriteLine(Sender);
                    //Console.WriteLine(Receiver[Receiver.Count - 1]);
                    break;
                default:
                    SendResponse("250 OK\r\n");
                    break;
            }
        }

        private void SendResponse(string Response)
        {
            byte[] buf = Encoding.ASCII.GetBytes(Response);
            client.GetStream().Write(buf, 0, buf.Length);
            Console.WriteLine(Response);
        }

        //считать сообщения и делать для них отдельные файлы ?
        private void SaveMassage(string message)
        {
            string[] mails = Directory.GetFiles(mailbox);
            string FileName = (mails.Length + 1).ToString() + ".txt";
            StreamWriter writer = new StreamWriter(mailbox + "/" + FileName, false, Encoding.ASCII);
            writer.Write(message);
            writer.Close();
        }

        //как-то отправлять получателю
        private void SendBack()
        {
            MailAddress From = new MailAddress(Sender);
            MailAddress To = new MailAddress(Receiver[Receiver.Count - 1]);
            string host = "smtp." + Receiver[Receiver.Count - 1].Substring(Receiver[Receiver.Count - 1].IndexOf("@") + 1);
            Console.WriteLine(host);
            Console.WriteLine(From.ToString());
            Console.WriteLine(To.ToString());
            MailMessage mes = new MailMessage(From, To);
            mes.Body = message.ToString();
            mes.IsBodyHtml = true;
            SmtpClient Smtp = new SmtpClient(host, 2525);
            Smtp.Credentials = new NetworkCredential(Sender, "666mefistofel");
            Smtp.EnableSsl = false;
            Smtp.Send(mes);
        }
    }
}
