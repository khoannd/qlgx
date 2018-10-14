using GxGlobal;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace DongBoDuLieu
{
    abstract class CompareMasterTable : Compare
    {
        public CompareMasterTable(string dir, string nameCSV) : base(dir, nameCSV)
        {
        }

        public abstract void importCacObject();
        public abstract void deleteObjectMaster();

        public void importObjectMaster(Dictionary<string, string> objectCSV, DataTable objectClient, string fieldID, string nameTable)
        {
            Dictionary<string, string> objectTrack = new Dictionary<string, string>();
            objectTrack.Add("updated", "false");
            objectTrack.Add("oldIdIsCsv", "true");
            objectTrack.Add("newId", "");
            objectTrack.Add("nowId", "");
            objectTrack.Add("oldId", "");
            if (objectClient == null)
            {
                //insert new
                objectTrack["oldId"] = objectCSV[fieldID];
                objectTrack["newId"] = Memory.Instance.GetNextId(GiaoDanConst.TableName, GiaoDanConst.MaGiaoDan, true).ToString();
                objectTrack["nowId"] = objectTrack["newId"];
                objectCSV[fieldID] = objectTrack["newId"];
                insert(objectCSV, nameTable);

            }
            else
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
                    objectCSV.Remove(fieldID);
                    update(objectCSV, nameTable, fieldID, objectTrack["nowId"]);
                }
                else
                {
                    objectTrack["oldIdIsCsv"] = "true";
                    objectTrack["newId"] = objectClient.Rows[0][fieldID].ToString();
                    objectTrack["oldId"] = objectCSV[fieldID];
                    objectTrack["nowId"] = objectClient.Rows[0][fieldID].ToString();
                }
            }
            ListTracks.Add(objectTrack);
        }
    }
}
