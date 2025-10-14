using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Oracle.ManagedDataAccess.Client;

namespace TestConsole
{
    class Program
    {
        static void Main(string[] args)
        {

            string constr = "User Id=LIS_ZO; Password=system32; Data Source=NEOSOFT;";
            string ProviderName = "Oracle.ManagedDataAccess.Client";

            DbProviderFactory factory = DbProviderFactories.GetFactory(ProviderName);

            using (DbConnection conn = factory.CreateConnection())
            {
                try
                {
                    conn.ConnectionString = constr;
                    conn.Open();

                    //Get MetaDataCollections and write to an XML file.
                    //This is equivalent to GetSchema()
                    //DataTable dtMetadata =
                    //  conn.GetSchema(DbMetaDataCollectionNames.MetaDataCollections);
                    //dtMetadata.WriteXml(ProviderName + "_MetaDataCollections.xml");

                    ////Get Restrictions and write to an XML file.
                    //DataTable dtRestrictions =
                    //  conn.GetSchema(DbMetaDataCollectionNames.Restrictions);
                    //dtRestrictions.WriteXml(ProviderName + "_Restrictions.xml");

                    ////Get DataSourceInformation and write to an XML file.
                    //DataTable dtDataSrcInfo =
                    //  conn.GetSchema(DbMetaDataCollectionNames.DataSourceInformation);
                    //dtDataSrcInfo.WriteXml(ProviderName + "_DataSourceInformation.xml");

                    ////DataTypes and write to an XML file.
                    //DataTable dtDataTypes =
                    //  conn.GetSchema(DbMetaDataCollectionNames.DataTypes);
                    //dtDataTypes.WriteXml(ProviderName + "_DataTypes.xml");

                    ////Get ReservedWords and write to an XML file.
                    //DataTable dtReservedWords =
                    //  conn.GetSchema(DbMetaDataCollectionNames.ReservedWords);
                    //dtReservedWords.WriteXml(ProviderName + "_ReservedWords.xml");

                    ////Get all the tables and write to an XML file.
                    //DataTable dtTables = conn.GetSchema("Tables");
                    //dtTables.WriteXml(ProviderName + "_Tables.xml");

                    ////Get all the views and write to an XML file.
                    //DataTable dtViews = conn.GetSchema("Views");
                    //dtViews.WriteXml(ProviderName + "_Views.xml");

                    ////Get all the columns and write to an XML file.
                    //DataTable dtColumns = conn.GetSchema("Columns");
                    //dtColumns.WriteXml(ProviderName + "_Columns.xml");
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    Console.WriteLine(ex.StackTrace);
                }

                //var message = "D     01 P001                   1417765    E0      t.saren/pp          1498971             001   184                      ";
                //var sampleNo = GetValue(32, 38, message);
                //string paramCode = GetValue(91, 93, message);

                //for (int i = 91; i < message.Length;)
                //{
                //    var LISParamCode = GetValue(i, i + 4, message).Trim();
                //    var LISParamValue = GetValue(i + 5, i + 11, message).Trim();
                //}
            }
        }


        private static string GetValue(int startindex, int lastindex, string message)
        {
            string value = "";
            var RecvChar = message.ToCharArray();
            for (int i = startindex; i <= lastindex; i++)
            {

                value += RecvChar[i].ToString();
            }
            return value;
        }
    }
}
