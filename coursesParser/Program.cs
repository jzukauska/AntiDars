using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;
using MySql.Data.MySqlClient;


namespace coursesParser
{
    class Program
    {

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

            

            // MySqlConnection comm = openDatabase();
            string url = "https://catalog.stcloudstate.edu/Catalog/ViewCatalog.aspx?pageid=viewcatalog&catalogid=8&topicgroupid=107";

            string  name, description,  credits;

            string preReqs, semestersOffered, shortVersion;
            int courseNumber;

            




            HtmlWeb web = new HtmlWeb();

            HtmlDocument document = web.Load(url);
            Console.WriteLine("Webpage loaded");

            

            HtmlNode node = document.DocumentNode.SelectSingleNode("//body//span[@id='ctl00_ctl00_mainLayoutContent_mainContent_pager']");
            var maxPages = node.LastChild.PreviousSibling.PreviousSibling.InnerText;// number of pages to concat onto thing
            int tryer;
            int.TryParse(maxPages, out tryer);
            HtmlNodeCollection collection = document.DocumentNode.SelectNodes("//table[@class='DeAcFormTable']");

            for (int j = 0; j < tryer; j++)
            {
                document = web.Load(url + $"&pg={j + 1 }");

                collection = document.DocumentNode.SelectNodes("//table[@class='DeAcFormTable']");
                for (int i = 0; i < collection.LongCount(); i += 2)
                {
                    //Section to be parsed
                    //------------------------------------------------------------------------------------------------------------------------
                    name = document.DocumentNode.SelectSingleNode($"//table[{i + 1}]//h3").InnerText;
                    description = document.DocumentNode.SelectSingleNode($"//table[{i + 2}]//td[2]").InnerText;
                    credits = document.DocumentNode.SelectSingleNode($"//table[{i + 1}]//td[2]").InnerText;
                    HtmlNodeCollection tempNode = document.DocumentNode.SelectNodes($"//table[{i + 2}]//tr[2]//a[@class='topictooltip']");
                    HtmlNodeCollection tempOffered = document.DocumentNode.SelectNodes($"//table[{i + 2}]//tr[last()]//li");

                    //------------------------------------------------------------------------------------------------------------------------
                    

                    
                    shortVersion = name.Split(' ')[0];
                    int.TryParse(name.Split(' ')[1].Replace(".", String.Empty), out courseNumber);
                    name = name.Substring(name.LastIndexOf('.') + 1);
                    credits = credits.Substring(credits.LastIndexOf(':') + 2);

                    Console.WriteLine("----------"+courseNumber);
                    //List<string> strList = new List<string>();
                    if (tempNode != null)
                    {

                        foreach (HtmlNode item in tempNode)
                        {
                            Console.WriteLine(item.InnerText);
                            //strList.Add(item.InnerText);
                        }


                    }
                    Console.WriteLine();

                    /* Console.WriteLine();
                     Console.WriteLine(description);
                     Console.WriteLine();
                     Console.WriteLine(credits);
                     Console.WriteLine();
                     List<string> strList = new List<string>();
                     if (tempNode != null)
                     {

                         foreach (HtmlNode item in tempNode)
                         {
                             Console.WriteLine(item.InnerHtml);
                             strList.Add(item.InnerText);
                         }


                     }
                     Console.WriteLine();





                     Console.WriteLine("---------------------------------------------------"); */



                }




            }




           






            //*[@id="aspnetForm"]/div[6]/div[2]/div[2]/table[2]/tbody/tr[1]/td[2]
            //foreach (HtmlNode node in nodeCollection)
            //{
            // Console.WriteLine(node. );
            //}

            //var conn = openDatabase();

            //Console.WriteLine("Starting collection");

            //MySqlCommand cmd = new MySqlCommand();
            //cmd.Connection = conn;

            //cmd.CommandText = string.Format("INSERT INTO idp.course_names(short_designator,actual_name,link) VALUES('{0}','{1}','{2}') on duplicate key update actual_name = values(actual_name), link = values(link);", i.shortName, i.name.Replace("'", "''"), i.link);


            //cmd.ExecuteNonQuery();


        }

        enum commands { exit, updateCategorys };
        static void Main(string[] args)
        {

            getUniversityCatalog();
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




