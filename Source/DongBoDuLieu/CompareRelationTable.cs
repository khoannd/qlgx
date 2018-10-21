using GxGlobal;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace DongBoDuLieu
{
    abstract class CompareRelationTable : Compare
    {
        public CompareRelationTable(string dir, string nameCSV) : base(dir, nameCSV)
        {
        }
        public abstract void importCacObject();
        public abstract void deleteObjectRelation();
        public void importObjectRelation(Dictionary<string, object>objectTrackMaster,string fieldID1,string fieldID2,List<Dictionary<string, object>>listObjectChange,string nameTable)
        {
            List<Dictionary<string, object>> listObjectCSV=null;
            if (compareString(objectTrackMaster["oldIdIsCsv"], "false"))
            {
                listObjectCSV = getListByID(Data, fieldID1, objectTrackMaster["newId"]);
            }
            else
            {
                listObjectCSV = getListByID(Data, fieldID1, objectTrackMaster["oldId"]);
            }
            if (listObjectCSV!=null && listObjectCSV.Count>0)
            {
                foreach (var data in listObjectCSV)
                {

                   try
                   {
                        int idObjectFK2 = findIdObjectClient(listObjectChange, data[fieldID2]);
                        if (idObjectFK2 == 0)
                        {
                            continue;
                        }
                        DataTable result = findWithID(fieldID1, objectTrackMaster["nowId"], fieldID2, idObjectFK2, nameTable);
                        Dictionary<string, object> objectTrackNew = new Dictionary<string, object>();
                        objectTrackNew[fieldID1] = objectTrackMaster["nowId"];
                        objectTrackNew[fieldID2] = idObjectFK2.ToString();
                        ListTracks.Add(objectTrackNew);
                        if (result != null && result.Rows.Count > 0)
                        {
                            if (compareDate(data["UpdateDate"], result.Rows[0]["UpdateDate"].ToString()))
                            {
                                //data.Remove(fieldID1);
                                //data.Remove(fieldID2);
                                DataTable rsUp = assignData(data, result,nameTable,fieldID1,fieldID2);
                                rsUp.TableName = nameTable;
                                update(rsUp);
                            }
                            continue;
                        }
                        //data[fieldID1] = objectTrackMaster["nowId"];
                        //data[fieldID2] = idObjectFK2.ToString();
                        DataTable rsAdd = assignDataAdd(data,fieldID1, objectTrackMaster["nowId"], nameTable,fieldID2,idObjectFK2);
                        update(rsAdd);
                    }
                   catch (System.Exception ex)
                   {
                        return;
                   }
                }
            }
        }
        public void deleteObjecChild(List<Dictionary<string, object>>listTracksObject,string fieldID1,string fieldID2,string nameTable)
        {
            DataTable tblObject = getAll(nameTable);
            if (tblObject!=null&&tblObject.Rows.Count>0)
            {
                foreach (DataRow item in tblObject.Rows)
                {
                    bool result = findObject(item,listTracksObject,fieldID1,fieldID2);
                    if (!result)
                    {
                        delete(string.Format(@"{0}=? AND {1}=?", fieldID1, fieldID2), nameTable, item[fieldID1],item[fieldID2]);
                    }
                }
            }
        }

        private bool findObject(DataRow objectDB, List<Dictionary<string, object>> listTracksObject, string fieldID1, string fieldID2)
        {
            if (listTracksObject!=null&& listTracksObject.Count>0)
            {
                foreach (var item in listTracksObject)
                {
                    if (compareString(item[fieldID1],objectDB[fieldID1].ToString())&&compareString(item[fieldID2],objectDB[fieldID2].ToString()))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private DataTable findWithID(string fieldID1,object idObjectFK1,string fieldID2, int idObjectFK2,string nameTable)
        {
            string query = string.Format(@"SELECT TOP 1 * FROM {0} WHERE {1}=? AND {2}=?", nameTable, fieldID1, fieldID2);
            DataTable tbl = null;
            try
            {
                tbl=Memory.GetData(query,idObjectFK1,idObjectFK2);
            }
            catch (System.Exception ex)
            {
                tbl = null;
            }
            return tbl;
        }

        
    }
}
