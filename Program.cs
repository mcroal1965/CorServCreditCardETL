﻿using System;
using System.IO;
using System.Xml;
using System.Configuration;
using System.Collections.Generic;
using System.Linq;
/*   CHANGE LOG
 * 9/01/2021  Trim card product and generate WF GIM Repair error files    
 * 7/29/2021  Don't divide interest rate by 100   
 * 6/22/2021  Default    
*/
namespace CorServCreditCardETL
{
    class Program
    {
        static void Main()
        {
            try
            {
                string filedate = DateTime.Today.ToString("yyyyMMdd");
                string useInfile = ConfigurationManager.AppSettings["Infile"].ToString();
                string useOutfile = ConfigurationManager.AppSettings["Outfile"].ToString();
                useOutfile = useOutfile.Replace("~", filedate);
                string useErrorfile = ConfigurationManager.AppSettings["ErrorFile"].ToString();
                useErrorfile = useErrorfile.Replace("~", filedate);
                string useAppSetting = ConfigurationManager.AppSettings["AppSetting"].ToString();
                string BackupYN = "N";
                try
                {
                    BackupYN = ConfigurationManager.AppSettings["Backup"].ToString();
                }
                catch
                {
                    Console.WriteLine("BackupYN value in config file is not configured properly and has been defaulted to N.");
                    BackupYN = "N";
                }

                try { File.Delete(useOutfile); } catch { }

                try
                {
                    XmlDocument XmlDoc = new XmlDocument();
                    XmlDoc.Load(useAppSetting);
                    XmlNodeList productlist = XmlDoc.GetElementsByTagName("Product");
                    XmlNodeList fieldlist = XmlDoc.GetElementsByTagName("Field");
                    string ProductName = "";
                    string ProductCode = "";
                    string ProductType = "";
                    string PType_06 = "";
                    string Name01out = "";
                    string Name02out = "";
                    string[] Name01split;
                    string[] Name02split;

                    StreamReader reading = File.OpenText(useInfile);
                    string str;
                    Int32 Counter = 0;

                    while ((str = reading.ReadLine()) != null)
                    {
                        if (Counter > 0)
                        {
                            string CreditCardProducts = str.Substring(str.IndexOf("Mastercard"), str.IndexOf("\t", str.IndexOf("Mastercard")) - str.IndexOf("Mastercard"));
                            CreditCardProducts = CreditCardProducts.Replace("®", "").Trim();
                            //Console.WriteLine(CreditCardProducts);
                            Int32 found = 0;

                            string sep = "\t";
                            string[] splitContent = str.Split(sep.ToCharArray());
                            List<string> linein = splitContent.ToList();

                            for (int i = 0; i < productlist.Count; i++)
                            {
                                ProductName = productlist[i].Attributes["Name"].Value;
                                ProductCode = productlist[i].Attributes["Code"].Value;
                                ProductCode = ProductCode.PadRight(100).Substring(0, 4);
                                ProductType = productlist[i].Attributes["Type"].Value;

                                if (CreditCardProducts == ProductName)
                                {
                                    string TaxId_01a = linein.ElementAt(1).Trim();
                                    TaxId_01a = TaxId_01a.PadRight(100).Substring(0, 9);
                                    string TaxId_01b = linein.ElementAt(2).Trim();
                                    TaxId_01b = TaxId_01b.PadRight(100).Substring(0, 9);
                                    string AcctNum_02 = linein.ElementAt(20).Trim();
                                    AcctNum_02 = AcctNum_02.PadRight(100).Substring(0, 30);
                                    string MajorCode_03 = "EXT ";
                                    string MinorCode_04 = ProductCode;
                                    string Name_05a = linein.ElementAt(3).Trim();
                                    string Name_05b = linein.ElementAt(4).Trim();
                                    if (ProductType == "Business") { PType_06 = "O"; } else { PType_06 = "P"; }
                                    if (PType_06 == "O")
                                    {
                                        Name01out = Name_05a.PadRight(100).Substring(0, 40);

                                        if (Name_05b.Trim() != "")
                                        {
                                            Name02out = Name_05b.PadRight(100).Substring(0, 40);
                                        }
                                    }
                                    else
                                    {
                                        sep = " ";
                                        Name01split = Name_05a.Split(sep.ToCharArray());
                                        int result1 = Name_05a.ToCharArray().Count(c => c == ' ');
                                        Name01out = Name01split.ElementAt(0).PadRight(100).Substring(0, 20) + Name01split.ElementAt(result1).PadRight(100).Substring(0, 20);

                                        if (Name_05b.Trim() != "")
                                        {
                                            Name02split = Name_05b.Split(sep.ToCharArray());
                                            int result2 = Name_05b.ToCharArray().Count(c => c == ' ');
                                            Name02out = Name02split.ElementAt(0).PadRight(100).Substring(0, 20) + Name02split.ElementAt(result2).PadRight(100).Substring(0, 20);
                                        }
                                    }
                                    string Gap_07 = "".PadRight(4);
                                    string AddrLine1_08 = linein.ElementAt(5).Trim();
                                    AddrLine1_08 = AddrLine1_08.PadRight(100).Substring(0, 40);
                                    string AddrLine2_09 = linein.ElementAt(6).Trim();
                                    AddrLine2_09 = AddrLine2_09.PadRight(100).Substring(0, 40);
                                    string Gap_10 = "".PadRight(40);
                                    string City_11 = linein.ElementAt(7).Trim();
                                    City_11 = City_11.PadRight(100).Substring(0, 30);
                                    string State_12 = linein.ElementAt(8).Trim();
                                    State_12 = State_12.PadRight(100).Substring(0, 2);
                                    string Zipcode_13 = linein.ElementAt(9).Trim();
                                    Zipcode_13 = Zipcode_13.PadRight(100).Substring(0, 5);
                                    string Gap_14 = "".PadRight(21);
                                    string Arecode_15 = linein.ElementAt(10).Trim();
                                    Arecode_15 = Arecode_15.PadRight(100).Substring(0, 3);
                                    string Exchange_16 = linein.ElementAt(10).Trim();
                                    Exchange_16 = Exchange_16.PadRight(100).Substring(3, 3);
                                    string Phone_17 = linein.ElementAt(10).Trim();
                                    Phone_17 = Phone_17.PadRight(100).Substring(6, 4);
                                    string Gap_18 = "".PadRight(10);
                                    string Intrate_19 = Convert.ToString(Convert.ToDecimal(linein.ElementAt(13))).PadRight(100).Substring(0, 15);
                                    string Gap_20 = "".PadRight(15);
                                    string AvailableCredit_21 = linein.ElementAt(17).Trim();
                                    AvailableCredit_21 = AvailableCredit_21.PadRight(100).Substring(0, 15);
                                    string DateOpen_22 = linein.ElementAt(18).Trim();
                                    DateOpen_22 = DateOpen_22.Replace("/", "").PadRight(100).Substring(0, 8);
                                    string MatDate_23 = linein.ElementAt(25).Trim();
                                    MatDate_23 = MatDate_23.Replace("/", "").PadRight(100).Substring(0, 8);
                                    string Gap_24 = "".PadRight(16);
                                    string Gap_25 = "".PadRight(8);
                                    string Gap_26 = "".PadRight(8);
                                    string CreditLimit_27 = linein.ElementAt(15).Trim(); //was 16
                                    CreditLimit_27 = CreditLimit_27.PadRight(100).Substring(0, 15);
                                    string Gap_28 = "".PadRight(8);
                                    string Gap_29 = "".PadRight(15);
                                    string PaymentDue_30 = linein.ElementAt(29).Trim();
                                    PaymentDue_30 = PaymentDue_30.Replace("/", "").PadRight(100).Substring(0, 8);
                                    string MinPayment_31 = linein.ElementAt(27).Trim();
                                    MinPayment_31 = MinPayment_31.PadRight(100).Substring(0, 15);
                                    string Gap_32 = "".PadRight(94);
                                    string Static_33 = "NOTE/BAL ";
                                    string CurrentBal_34 = linein.ElementAt(14).Trim(); //was 15
                                    CurrentBal_34 = CurrentBal_34.PadRight(100).Substring(0, 15);
                                    string Datadate_35 = linein.ElementAt(0).Trim();
                                    Datadate_35 = Datadate_35.Replace("/", "").PadRight(100).Substring(0, 8);
                                    string Gap_36 = "".PadRight(1389);

                                    if (linein.ElementAt(19) == "Open" && linein.ElementAt(20) != "")
                                    {
                                        string lineout = TaxId_01a + AcctNum_02 + MajorCode_03 + MinorCode_04 + Name01out + PType_06 + Gap_07 + AddrLine1_08 + AddrLine2_09 + Gap_10 + City_11 + State_12 + Zipcode_13 + Gap_14 + Arecode_15 + Exchange_16 + Phone_17 + Gap_18 + Intrate_19 + Gap_20 + AvailableCredit_21 + DateOpen_22 + MatDate_23 + Gap_24 + Gap_25 + Gap_26 + CreditLimit_27 + Gap_28 + Gap_29 + PaymentDue_30 + MinPayment_31 + Gap_32 + Static_33 + CurrentBal_34 + Datadate_35 + Gap_36;
                                        /*
                                        int x = 0;
                                        while (x < 29)
                                        {
                                            Console.WriteLine(" " + x + " " + linein.ElementAt(x).Trim());
                                            x++;
                                        }
                                        */
                                        File.AppendAllText(useOutfile, lineout.Replace("\n", null).Replace("\r", null) + "\r\n");

                                        if (TaxId_01b.Trim() != "")
                                        {
                                            string lineout2 = TaxId_01b + AcctNum_02 + MajorCode_03 + MinorCode_04 + Name02out + PType_06 + Gap_07 + AddrLine1_08 + AddrLine2_09 + Gap_10 + City_11 + State_12 + Zipcode_13 + Gap_14 + Arecode_15 + Exchange_16 + Phone_17 + Gap_18 + Intrate_19 + Gap_20 + AvailableCredit_21 + DateOpen_22 + MatDate_23 + Gap_24 + Gap_25 + Gap_26 + CreditLimit_27 + Gap_28 + Gap_29 + PaymentDue_30 + MinPayment_31 + Gap_32 + Static_33 + CurrentBal_34 + Datadate_35 + Gap_36;
                                            File.AppendAllText(useOutfile, lineout2.Replace("\n", null).Replace("\r", null) + "\r\n");
                                        }
                                    }
                                    else
                                    {
                                        if (linein.ElementAt(19) != "Open")
                                        {
                                            if (Name01out == "".PadRight(100).Substring(0, 40) || Name01out is null)
                                            {
                                                string CardLine = "WF GIM Repair".PadRight(100).Substring(0, 20) + "NOTIFY_CARD_NOT_OPEN".PadRight(100).Substring(0, 25) + "||" + AcctNum_02 + "|" + TaxId_01a + "-" + Name02out + "\n";
                                                File.AppendAllText(useErrorfile, CardLine);
                                            }
                                            else
                                            {
                                                string CardLine = "WF GIM Repair".PadRight(100).Substring(0, 20) + "NOTIFY_CARD_NOT_OPEN".PadRight(100).Substring(0, 25) + "||" + AcctNum_02 + "|" + TaxId_01a + "-" + Name01out + "\n";
                                                File.AppendAllText(useErrorfile, CardLine);
                                            }
                                        }
                                    }
                                    found = 1;
                                }
                            }

                            if (found == 0)
                            {
                                string ErrorLine = "WF GIM Repair".PadRight(100).Substring(0, 20) + "Notification".PadRight(100).Substring(0, 25) + "||CORSERV|Product name " + CreditCardProducts + " Not found in " + useAppSetting;
                                File.AppendAllText(useErrorfile, ErrorLine);
                                
                                Console.WriteLine(ErrorLine);
                                Console.WriteLine("Press any key to exit.");
                                Console.ReadKey();
                                Environment.Exit(9);
                            }
                        }

                        Counter++;
                    }
                    reading.Close();

                    if (BackupYN is "Y")
                    {
                        string Renamefile = useInfile.Substring(0, useInfile.LastIndexOf(@"\") + 1) + filedate + '_' + useInfile.Substring(useInfile.LastIndexOf(@"\") + 1, (useInfile.LastIndexOf(".") - useInfile.LastIndexOf(@"\"))) + "txt";
                        File.Copy(useInfile, Renamefile, true);
                    }

                    File.Delete(useInfile);
                }
                catch (Exception ex)
                {
                    string error = ex.ToString();
                    string ErrorLine = "WF GIM Repair".PadRight(100).Substring(0, 20) + "Notification".PadRight(100).Substring(0, 25) + "||CORSERV|" + error.Replace("\n", " ").Replace("\r", " ").Substring(0, 230);
                    File.AppendAllText(useErrorfile, ErrorLine);

                    Console.WriteLine("Error: " + ex);
                    Console.WriteLine("Press any key to exit.");
                    Console.ReadKey();
                    Environment.Exit(1);
                }
                finally
                {
                    Environment.Exit(0);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Config file does not exist or does not meet requirements.");
                Console.WriteLine("Error: " + ex);
                Console.WriteLine("Press any key to exit.");
                Console.ReadKey();
                Environment.Exit(1);
            }
        }
    }
}
