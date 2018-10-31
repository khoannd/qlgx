using GxGlobal;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace DongBoDuLieu
{
    public abstract class CompareMasterTable : Compare
    {
        public CompareMasterTable(string dir, string nameCSV) : base(dir, nameCSV)
        {
        }
        
        public abstract void importCacObject();
        public abstract bool deleteObjectMaster(Dictionary<string, object> objectCSV, DataTable item);

        public void importObjectMaster(Dictionary<string, object> objectCSV, DataTable objectClient, string fieldID, string nameTable)
        {
            Dictionary<string, object> objectTrack = new Dictionary<string, object>();
            objectTrack.Add("updated", "false");
            objectTrack.Add("oldIdIsCsv", "true");
            objectTrack.Add("newId", "");
            objectTrack.Add("nowId", "");
            objectTrack.Add("oldId", "");
            if (objectClient == null)
            {
               try
               {
                    //insert new
                    objectTrack["oldId"] = objectCSV[fieldID];
                    objectTrack["newId"] = Memory.Instance.GetNextId(nameTable, fieldID, true).ToString();
                    objectTrack["nowId"] = objectTrack["newId"];
                    //objectCSV[fieldID] = objectTrack["newId"];
                    DataTable resultAdd=assignDataAdd(objectCSV,fieldID,objectTrack["nowId"], nameTable);
                    update(resultAdd);
                }
               catch (System.Exception ex)
               {
                    return;
               }

            }
            else
            {
               try
               {
                    //update
                    processDataNull(objectCSV, objectClient);
                    objectTrack["updated"] = "true";
                    if (compareDate(objectCSV["UpdateDate"], objectClient.Rows[0]["UpdateDate"].ToString()))
                    {
                        objectTrack["newId"] = objectCSV[fieldID];
                        objectTrack["oldId"] = objectClient.Rows[0][fieldID].ToString();
                        objectTrack["nowId"] = objectClient.Rows[0][fieldID].ToString();
                        objectTrack["oldIdIsCsv"] = "false";
                        //objectCSV[fieldID] = objectTrack["nowId"];
                        DataTable result=assignData(objectCSV,objectClient,nameTable,fieldID);
                        update(result);
                    }
                    else
                    {
                        objectTrack["oldIdIsCsv"] = "true";
                        objectTrack["newId"] = objectClient.Rows[0][fieldID].ToString();
                        objectTrack["oldId"] = objectCSV[fieldID];
                        objectTrack["nowId"] = objectClient.Rows[0][fieldID].ToString();
                    }
                }
               catch (System.Exception ex)
               {
                    return;
               }
            }
            ListTracks.Add(objectTrack);
        }

       
    }
}
