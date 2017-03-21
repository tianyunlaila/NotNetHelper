using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Colors;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;

namespace NotNetHelper
{
    /// <summary>
    /// 图层操作类
    /// </summary>
    public static class LayerTools
    {
        /// <summary>
        /// 添加不存在的图层
        /// </summary>
        /// <param name="db"></param>
        /// <param name="layerName">图层名</param>
        /// <returns></returns>
        public static ObjectId AddLayer(this Database db, string layerName)
        {
            ObjectId layerId = ObjectId.Null;
            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                LayerTable lt = (LayerTable)trans.GetObject(db.LayerTableId, OpenMode.ForWrite);
                if (!lt.Has(layerName))
                {
                    LayerTableRecord ltr = new LayerTableRecord();
                    ltr.Name = layerName;
                    layerId = lt.Add(ltr);
                    trans.AddNewlyCreatedDBObject(ltr, true);
                }
                trans.Commit();
            }
            return layerId;
        }
        /// <summary>
        /// 设置图层索引颜色
        /// </summary>
        /// <param name="db"></param>
        /// <param name="layerName">图层名</param>
        /// <param name="colorIndex">索引颜色</param>
        /// <returns></returns>
        public static bool SetLayerColor(this Database db, string layerName, int colorIndex)
        {
            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                LayerTable lt = (LayerTable)db.LayerTableId.GetObject(OpenMode.ForRead);
                if (!lt.Has(layerName)) return false;
                ObjectId layerId = lt[layerName];
                LayerTableRecord ltr = (LayerTableRecord)layerId.GetObject(OpenMode.ForWrite);
                ltr.Color = Color.FromColorIndex(ColorMethod.ByAci, (short)colorIndex);
                ltr.DowngradeOpen();
                trans.Commit();
            }
            return true;
        }
        /// <summary>
        /// 设置图层rgb颜色
        /// </summary>
        /// <param name="db"></param>
        /// <param name="layerName"></param>
        /// <param name="RedColor">R</param>
        /// <param name="GreenColor">G</param>
        /// <param name="BlueColor">B</param>
        /// <returns></returns>
        public static bool SetLayerColor(this Database db, string layerName, int RedColor, int GreenColor, int BlueColor)
        {
            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                LayerTable lt = (LayerTable)db.LayerTableId.GetObject(OpenMode.ForRead);
                if (!lt.Has(layerName)) return false;
                ObjectId layerId = lt[layerName];
                LayerTableRecord ltr = (LayerTableRecord)layerId.GetObject(OpenMode.ForWrite);
                ltr.Color = Color.FromRgb(Convert.ToByte(RedColor), Convert.ToByte(GreenColor), Convert.ToByte(BlueColor));
                ltr.DowngradeOpen();
                trans.Commit();
            }
            return true;
        }
        /// <summary>
        /// 设置当前层
        /// </summary>
        /// <param name="db"></param>
        /// <param name="layerName">图层名</param>
        /// <returns></returns>
        public static bool SetCurrentLayer(this Database db, string layerName)
        {
            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                LayerTable lt = (LayerTable)db.LayerTableId.GetObject(OpenMode.ForRead);
                if (!lt.Has(layerName)) return false;
                ObjectId layerId = lt[layerName];
                if (db.Clayer == layerId) return false;
                db.Clayer = layerId;
                trans.Commit();
            }
            return true;
        }
        /// <summary>
        /// 删除图层及内部数据
        /// </summary>
        /// <param name="db"></param>
        /// <param name="layerName"></param>
        public static void DeleteLayer(this Database db, string layerName)
        {
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                LayerTable lt = (LayerTable)trans.GetObject(db.LayerTableId, OpenMode.ForWrite);
                LayerTableRecord currentLayer = (LayerTableRecord)trans.GetObject(db.Clayer, OpenMode.ForRead);
                if (currentLayer.Name.ToLower() == layerName.ToLower())
                    ed.WriteMessage("\n不能删除当前层");
                else
                {
                    LayerTableRecord ltr = new LayerTableRecord();
                    if (lt.Has(layerName))
                    {
                        ltr = trans.GetObject(lt[layerName], OpenMode.ForWrite) as LayerTableRecord;
                        if (ltr.IsErased)
                            ed.WriteMessage("\n此层已经被删除");
                        else
                        {
                            ObjectIdCollection idCol = new ObjectIdCollection();
                            idCol.Add(ltr.ObjectId);
                            db.Purge(idCol);
                            if (idCol.Count == 0)
                                ed.WriteMessage("\n不能删除包含对象的图层");
                            else ltr.Erase();
                        }
                    }
                    else ed.WriteMessage("\n没有此图层");
                }
                trans.Commit();
            }
        }
        /// <summary>
        /// 设置图层关闭
        /// </summary>
        /// <param name="db"></param>
        /// <param name="layerName">图层名</param>
        /// <returns></returns>
        public static bool SetLayerOff(this Database db, string layerName)
        {
            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                LayerTable lt = (LayerTable)db.LayerTableId.GetObject(OpenMode.ForWrite);
                LayerTableRecord currentLayer = (LayerTableRecord)trans.GetObject(db.Clayer, OpenMode.ForRead);
                if (currentLayer.Name.ToLower() == layerName.ToLower())
                    return false;
                else
                {
                    if (!lt.Has(layerName)) return false;
                    LayerTableRecord ltr = (LayerTableRecord)trans.GetObject(lt[layerName], OpenMode.ForWrite);
                    ltr.IsOff = true;
                    trans.Commit();
                }
            }
            return true;
        }
        /// <summary>
        /// 设置图层开启
        /// </summary>
        /// <param name="db"></param>
        /// <param name="layerName">图层名</param>
        /// <returns></returns>
        public static bool SetLayerOn(this Database db, string layerName)
        {
            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                LayerTable lt = (LayerTable)db.LayerTableId.GetObject(OpenMode.ForWrite);
                if (!lt.Has(layerName)) return false;
                LayerTableRecord ltr = (LayerTableRecord)trans.GetObject(lt[layerName], OpenMode.ForWrite);
                ltr.IsOff = false;
                trans.Commit();
            }
            return true;
        }
        /// <summary>
        /// 获取图层名列表
        /// </summary>
        /// <param name="db"></param>
        /// <returns></returns>
        public static List<string> GetLayerNameList(this Database db)
        {
            List<string> layerList = new List<string>();
            LayerTable lyTable;
            LayerTableRecord ltLayerTableRecord;
            using (Transaction tr = db.TransactionManager.StartTransaction()) //using,自动调用Dispose方法
            {
                //处理过程
                lyTable = (LayerTable)tr.GetObject(db.LayerTableId, OpenMode.ForRead);
                foreach (ObjectId layerId in lyTable)
                {
                    ltLayerTableRecord = (LayerTableRecord)tr.GetObject(layerId, OpenMode.ForRead);
                    layerList.Add(ltLayerTableRecord.Name.ToString());
                }
                tr.Commit();
                //事务提交
            }
            return layerList;
        }
    }
}
