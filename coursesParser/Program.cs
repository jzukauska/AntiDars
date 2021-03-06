﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;
using MySql.Data.MySqlClient;
using System.Text.RegularExpressions;

namespace coursesParser
{
    class Program
    {
        private static Regex digitsOnly = new Regex(@"[^\d]");
        private static Regex rgx = new Regex("[^a-zA-Z0-9 -]");

        private static MySqlConnection openDatabase()
        {
            string userName = "";
            string pass = "";

            Console.WriteLine("Database userName: ");
            userName = Console.ReadLine();



            Console.Write("Enter your password: ");
            ConsoleKeyInfo key;

            do
            {
                key = Console.ReadKey(true);

                // Backspace Should Not Work
                if (key.Key != ConsoleKey.Backspace && key.Key != ConsoleKey.Enter)
                {
                    pass += key.KeyChar;
                    Console.Write("*");
                }
                else
                {
                    if (key.Key == ConsoleKey.Backspace && pass.Length > 0)
                    {
                        pass = pass.Substring(0, (pass.Length - 1));
                        Console.Write("\b \b");
                    }
                }
            }
            // Stops Receving Keys Once Enter is Pressed
            while (key.Key != ConsoleKey.Enter);

            Console.WriteLine();





            string myConnection = $"datasource=loworbitnetwork.tk;port=3306;username={userName};password={pass};";
            MySqlConnection myConn = new MySqlConnection(myConnection);
            MySqlDataAdapter mydataAdapter = new MySqlDataAdapter();

            try
            {
                myConn.Open();
                Console.WriteLine("Database opened ");
                return myConn;

            }
            catch
            {
                myConn.Close();
                Console.WriteLine("Database closed");
                return null;
            }

        }

        private static void regenCourseList()
        {

            List<Course> courses = new List<Course>();

            HtmlWeb web = new HtmlWeb();
            string url = "https://catalog.stcloudstate.edu/Catalog/ViewCatalog.aspx?pageid=viewcatalog&catalogid=8&loaduseredits=False";
            HtmlDocument document = web.Load(url);
            Console.WriteLine("Webpage loaded");
            HtmlNodeCollection nodeCollection = document.DocumentNode.SelectNodes("//ul[@class='plainlist']//li");

            Console.WriteLine("Starting course collection..");
            foreach (HtmlNode node in nodeCollection)
            {
                string fullName = "";
                string partialName = "";
                string link = "";

                string tempString = node.InnerText;


                fullName = tempString.Split('(').ElementAt(0);
                partialName = tempString.Split('(').ElementAt(1);
                partialName = partialName.Split(')').FirstOrDefault();
                link = "catalog.stcloudstate.edu" + node.Descendants("a").FirstOrDefault().Attributes["href"].Value;



                Course course = new Course()
                {
                    name = fullName,
                    shortName = partialName,
                    link = link
                };
                courses.Add(course);
                Console.Write(".");

            }
            Console.WriteLine();
            Console.WriteLine("List has been collected");


            Console.WriteLine("Do you want to update the database with this data?");
            ConsoleKeyInfo key = Console.ReadKey();
            Console.WriteLine();
            if (key.KeyChar == 'y')
            {
                MySqlConnection conn = openDatabase();

                if (conn != null)
                {

                    Console.WriteLine("MySQL version : {0}", conn.ServerVersion);

                    try
                    {
                        MySqlCommand cmd = new MySqlCommand();
                        cmd.Connection = conn;


                        int tempNum;
                        foreach (Course i in courses)
                        {
                            cmd.CommandText = string.Format("INSERT INTO idp.course_names(short_designator,actual_name,link) VALUES('{0}','{1}','{2}') on duplicate key update actual_name = values(actual_name), link = values(link);", i.shortName, i.name.Replace("'", "''"), i.link);


                            tempNum = cmd.ExecuteNonQuery();



                        }
                    }
                    catch (Exception)
                    {

                        throw;
                    }
                    finally
                    {
                        conn.Close();
                    }
                }





            }
            Console.WriteLine("Database update complete");


        }
        private static void getUniversityCatalog()
        {


            Console.WriteLine("Login to get links from database");
            var conn = openDatabase();

            Console.WriteLine("Starting collection");

            MySqlCommand command = new MySqlCommand();
            command.Connection = conn;


            List<string> values = new List<string>();



            string sql = "SELECT link FROM idp.course_names";
            command.CommandText = sql;
            MySqlDataReader reader = command.ExecuteReader();

            
            
            while (reader.Read())
            {

               
                
                values.Add(digitsOnly.Replace(reader.GetString(0), "").Substring(1));
                //Console.WriteLine(digitsOnly.Replace(reader.GetString(0),"").Substring(1));

            }

            conn.Close();
            reader.Close();

            MySqlConnection comm = openDatabase();
            MySqlCommand cmd = new MySqlCommand();
            cmd.Connection = comm;
            foreach (string item in values)
            {
               // Console.WriteLine(item);

                 
                string url =  $"http://catalog.stcloudstate.edu/Catalog/ViewCatalog.aspx?pageid=viewcatalog&catalogid=8&topicgroupid={item}";
               // string url = $"http://catalog.stcloudstate.edu/Catalog/ViewCatalog.aspx?pageid=viewcatalog&catalogid=8&topicgroupid=107";
                Console.WriteLine("Current url: " + url);
                string name, description, credits;

                string shortVersion,href;
                int courseNumber;






                HtmlWeb web = new HtmlWeb();

                HtmlDocument document = web.Load(url);
                Console.WriteLine("Webpage loaded");



                HtmlNode node = document.DocumentNode.SelectSingleNode("//body//span[@id='ctl00_ctl00_mainLayoutContent_mainContent_pager']");
                string maxPages;
                if (node != null)
                {
                     maxPages = node.LastChild.PreviousSibling.PreviousSibling.InnerText;
                }
                else
                {
                    maxPages = "1";
                }
                // number of pages to concat onto thing
                int tryer;
                int.TryParse(maxPages, out tryer);
                HtmlNodeCollection collection = document.DocumentNode.SelectNodes("//table[@class='DeAcFormTable']");
                //Number of pages to grab 
                for (int j = 0; j < tryer; j++)
                {
                    document = web.Load(url + $"&pg={j + 1 }");
                    //Amount of tables on the page
                    collection = document.DocumentNode.SelectNodes("//table[@class='DeAcFormTable']");
                    for (int i = 0; i < collection.LongCount(); i += 2)
                    {
                        //Section to be parsed
                        //------------------------------------------------------------------------------------------------------------------------
                        name = document.DocumentNode.SelectSingleNode($"//table[{i + 1}]//h3").InnerText;
                        description = document.DocumentNode.SelectSingleNode($"//table[{i + 2}]//td[2]").InnerText;
                        credits = document.DocumentNode.SelectSingleNode($"//table[{i + 1}]//td[2]").InnerText;
                        HtmlNodeCollection tempPrereqs = document.DocumentNode.SelectNodes($"//table[{i + 2}]//tr[2]//a[@class='topictooltip']");
                        HtmlNodeCollection tempOffered = document.DocumentNode.SelectNodes($"//table[{i + 2}]//tr[last()]//li");

                        //------------------------------------------------------------------------------------------------------------------------



                        shortVersion = name.Split(' ')[0];
                        int.TryParse(name.Split(' ')[1].Replace(".", String.Empty), out courseNumber);
                        name = name.Substring(name.LastIndexOf('.') + 1).TrimStart();
                        credits = credits.Substring(credits.LastIndexOf(':') + 2);
                        
                        Console.WriteLine(name + " " + courseNumber);

                        //Database push
                        //---------------------------------------------------- 
                        Console.WriteLine("Trying to input this into the database:");
                        Console.WriteLine($"Course number: {courseNumber}, Short Version: {shortVersion}, Name: {name}\nDescription: {description}");
                        cmd.CommandText = string.Format("INSERT IGNORE INTO idp.course_collection(course_number,short,name,description) VALUES('{0}','{1}','{2}','{3}')", courseNumber, rgx.Replace(shortVersion, ""), rgx.Replace(name, ""), rgx.Replace(description, ""));
                        cmd.ExecuteNonQuery();

                        if (tempOffered != null )
                        {
                            foreach (HtmlNode item1 in tempOffered)
                            {
                                Console.WriteLine(item1.InnerText + ", ");
                                if (!item1.InnerText.Contains("GOAL"))
                                {
                                    cmd.CommandText = string.Format("INSERT  INTO idp.seasons(course_number,seasons) VALUES('{0}','{1}')", courseNumber, item1.InnerText);

                                    cmd.ExecuteNonQuery();
                                }
                                else
                                {
                                    Console.WriteLine($"********************{item1.InnerText}**************************");
                                }
                            }
                        }
                        else
                        {
                            cmd.CommandText = string.Format("INSERT INTO idp.seasons(course_number,seasons) VALUES('{0}','{1}')", courseNumber, "Demand");
                            cmd.ExecuteNonQuery();
                        }


                        if (tempPrereqs != null)
                        {
                            foreach (HtmlNode item1 in tempPrereqs)
                            {
                                Console.Write(item1.InnerText + ", ");
                                cmd.CommandText = string.Format("INSERT IGNORE INTO idp.prereqs(course_number,prereq,link) VALUES('{0}','{1}','{2}')", courseNumber, item1.InnerText, item1.Attributes["href"].Value);

                                cmd.ExecuteNonQuery();
                            }
                        }
                        Console.WriteLine();

                    }
                    
                }

            }

            comm.Close();
        }

        enum commands { exit, updateCategorys };
        static void Main(string[] args)
        {

            getUniversityCatalog();

            //regenCourseList();
            //bool flag = true;

            //String[] commandArray = new String[] { 0 + " to exit", 1 + " for Updateing course categories" };
            //do
            //{

            // Console.WriteLine();
            // Console.WriteLine("Enter choice ");
            // foreach (String s in commandArray)
            // {
            // Console.WriteLine(s);
            // }
            // Console.Write("-> ");

            // switch (Console.ReadLine())
            // {
            // case "0":
            // {
            // flag = false;
            // Console.WriteLine("Exiting");
            // break;
            // }
            // case "1":
            // {
            // regenCourseList();
            // break;
            // }
            // default:
            // {
            // Console.WriteLine("Not a Valid selection");
            // break;

            // }
            // }

            //} while (flag);

        }
    }
}




