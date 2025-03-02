// using System;
// using UnityEngine;
// // using Newtonsoft.Json;

// public static class SerializationHandler
// {
//     public delegate string ConversionPipeline(string input);

//     // public static string SerializeObject<T>(T serializableObject, ConversionPipeline conversionPipeline = null)
//     // {
//     //     try
//     //     {
//     //         string jsonString = JsonConvert.SerializeObject(serializableObject);
//     //         if (conversionPipeline != null)
//     //             return conversionPipeline(jsonString);
//     //         return jsonString;
//     //     }
//     //     catch (Exception e)
//     //     {
//     //         Debug.LogError(e);
//     //         return null;
//     //     }
//     // }

//     public static T DeserializeObject<T>(string jsonString, ConversionPipeline conversionPipeline = null)
//     {
//         return default(T);
//         try
//         {
//             if (conversionPipeline != null)
//                 jsonString = conversionPipeline(jsonString);
//             return JsonConvert.DeserializeObject<T>(jsonString);
//         }
//         catch (Exception e)
//         {
//             Debug.LogError(e);
//             return default(T);
//         }
//     }

// }
