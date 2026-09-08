$ErrorActionPreference = 'Stop'
# Real inventory/order/progression source with non-Unity stand-ins; no Unity build.
$root = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../..'))
$source = @'
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace UnityEngine {
 public class MonoBehaviour { protected object gameObject; protected void Destroy(object value) {} }
 public class ScriptableObject {}
 public class Sprite {}
 public class SerializeField : Attribute {}
 public class HideInInspector : Attribute {}
 public class MinAttribute : Attribute { public MinAttribute(float value) {} }
 public class RangeAttribute : Attribute { public RangeAttribute(float min,float max) {} }
 public class CreateAssetMenuAttribute : Attribute { public string fileName; public string menuName; }
 public static class Debug { public static void LogWarning(object value) {} }
 public static class Mathf { public static int Clamp(int v,int min,int max) { return Math.Min(max,Math.Max(min,v)); } }
}
namespace Florist.Merge {
 public class ItemData { public Sprite[] Sprites = new Sprite[6]; public int ItemType; }
 public class ProductionItemData { public int maxLevel=1; public int baseValue; public string itemName; public Sprite itemSprite; }
 public class CustomerData {}
 public class MergeEconomyConfig { public int InventoryCapacity=24; }
 public class RecipeIngredient { public ItemData itemData; public int requiredLevel; public int requiredCount; }
 public static class LogicChecks {
  static int passed;
  static void Check(bool value,string name) { if(!value) throw new Exception(name); passed++; }
  public static string Run() {
   var raw=new ItemData(); var inv=new InventoryManager();
   Check(!inv.AddItem(raw,0) && !inv.AddItem(raw,7),"invalid raw levels");
   Check(inv.AddItem(raw,2,2),"add low-level stack");
   Check(inv.TryTakeRegular(inv.GetInventory()[0]) && inv.GetInventory()[0].count==1,"withdraw one");
   Check(!inv.TryTakeRegular(new InventoryItem(raw,2)),"reject foreign stack");
   inv.ClearInventory(); inv.AddItem(raw,5);
   var overlapping=new List<RecipeIngredient>{new RecipeIngredient{itemData=raw,requiredLevel=2,requiredCount=1},new RecipeIngredient{itemData=raw,requiredLevel=5,requiredCount=1}};
   Check(!inv.TryConsumeIngredients(overlapping) && inv.GetInventory()[0].count==1,"overlap cannot double spend or partially consume");
   inv.AddItem(raw,2);
   Check(inv.TryConsumeIngredients(overlapping) && inv.GetInventory().Count==0,"reserve stricter ingredient first");
   Check(!inv.TryConsumeIngredients(new List<RecipeIngredient>()),"empty recipe cannot consume");
   var p=new ProductionItemData{itemName="Papatya",baseValue=125};var q=new ProductionItemData{itemName="Starlice",baseValue=240};
   inv.AddProductionItem(p,1,2);
   var requests=new List<OrderRequest>{new OrderRequest{item=p,count=2},new OrderRequest{item=q,count=1}};
   Check(!inv.TryConsumeProducts(requests) && inv.HasProductionItem(p,2),"missing second product leaves first intact");
   var duplicated=new List<OrderRequest>{new OrderRequest{item=p,count=2},new OrderRequest{item=p,count=1}};
   Check(!inv.TryConsumeProducts(duplicated),"duplicate order lines cannot double spend");
   inv.AddProductionItem(q,1);
   var order=new Order(new CustomerData(),requests,1);
   Check(order.reward==490 && order.CanBeCompleted(inv) && order.GetOrderDescription().Contains("2 × Papatya"),"multi-item reward and completion");
   Check(inv.TryConsumeProducts(requests) && inv.GetInventory().Count==0,"atomic product settlement");
   order.isCompleted=true;inv.AddProductionItem(p,1,2);inv.AddProductionItem(q,1);
   Check(!order.CanBeCompleted(inv),"completed order cannot repeat");
   inv.ClearInventory();inv.AddItem(raw,1,int.MaxValue);
   Check(!inv.AddItem(raw,1) && inv.GetInventory()[0].count==int.MaxValue,"stack overflow is rejected");
   inv.ClearInventory();for(int i=0;i<24;i++) inv.AddItem(new ItemData(),1);
   Check(!inv.AddItem(raw,1) && inv.GetInventory().Count==24,"capacity enforced");
   Check(inv.AddItem(raw,1,1,true) && inv.GetInventory().Count==25,"legacy over-capacity restore retains items");
   Check(inv.AddItem(raw,1) && inv.GetInventory()[24].count==2,"existing stack accepts at capacity");
   var cfg=new MergeProgressionConfig();foreach(int n in new[]{0,4,10,20,35}) cfg.Stages.Add(new MergeOrderStage{CompletedOrders=n,Name=n.ToString()});
   Check(cfg.GetStage(3).CompletedOrders==0 && cfg.GetStage(4).CompletedOrders==4 && cfg.GetStage(10).CompletedOrders==10,"unlock boundaries");
   Check(cfg.GetNextThreshold(4)==10 && cfg.GetNextThreshold(35)==-1,"next progress threshold");
   return passed+" logic checks passed";
  }
 }
}
'@
foreach ($name in @('InventoryManager','InventoryItem','Order','MergeProgressionConfig')) {
 $code=[IO.File]::ReadAllText((Join-Path $root ('Assets/_Merge/Scripts/'+$name+'.cs')))
 $source += [regex]::Replace($code,'(?m)^using [^;]+;\r?\n','')
}
Add-Type -TypeDefinition $source -IgnoreWarnings -WarningAction SilentlyContinue
[Florist.Merge.LogicChecks]::Run()

