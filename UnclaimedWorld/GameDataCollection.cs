using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.AllGameData;

namespace UWGame.SimSide
{
    public class GameDataCollection<T> : Dictionary<string, T>, IGameDataCollection where T: IGameData
    {
        private int order;
        public int Order
        {
            get
            {
                return order;
            }
        }

        public IGameData Get(string key)
        {
            return base[key];
        }

        public GameDataCollection(Dictionary<Type, IGameDataCollection> allGameDataCollections, DataLoaderQueueState /*int*/ order)
        {
            this.order = (int)order;
                        
            allGameDataCollections[typeof(T)] = this;

        }

        public void PreDataCompleteValidate(Dictionary<string, List<string>> allPreInitValidationErrors) // ref List<string> errors)
        {
            List<string> validationErrors;

            string typeName = typeof(T).Name;

            foreach (var item in this)
            {
                validationErrors = null;

                item.Value.PreDataCompleteValidate(ref validationErrors);

                if (validationErrors != null)
                {
                    allPreInitValidationErrors.Add(typeName /*fileDescriptor*/ + "/" + item.Key, validationErrors);
                }
            }

        }

        public void PostDataCompleteInitialize()
        {
            foreach (var item in this)
            {
                item.Value.PostDataCompleteInitialize();
            }

        }

        public void PostDataCompleteValidate(Dictionary<string, List<string>> allPostInitValidationErrors) // ref List<string> errors)
        {
            List<string> validationErrors;

            string typeName = typeof(T).Name;

            foreach (var item in this)
            {
                validationErrors = null;

               /* if (item.DeleteRecord == false)
                {*/
                    item.Value.PostDataCompleteValidate(ref validationErrors);

                    if (validationErrors != null)
                    {
                        allPostInitValidationErrors.Add(typeName /*fileDescriptor*/ + "/" + item.Key, validationErrors);
                    }

               // }
            }

        }

    }
}
