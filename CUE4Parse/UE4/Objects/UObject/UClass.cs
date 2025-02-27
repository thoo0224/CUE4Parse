﻿using System;
using System.Collections.Generic;
using CUE4Parse.UE4.Assets;
using CUE4Parse.UE4.Assets.Readers;
using CUE4Parse.UE4.Versions;
using Newtonsoft.Json;
using Serilog;
using static CUE4Parse.UE4.Versions.EUnrealEngineObjectUE4Version;

namespace CUE4Parse.UE4.Objects.UObject
{
    [SkipObjectRegistration]
    public class UClass : UStruct
    {
        /** Used to check if the class was cooked or not */
        public bool bCooked;

        /** Class flags; See EClassFlags for more information */
        public uint ClassFlags; // EClassFlags

        /** The required type for the outer of instances of this class */
        public FPackageIndex ClassWithin;

        /** This is the blueprint that caused the generation of this class, or null if it is a native compiled-in class */
        public FPackageIndex ClassGeneratedBy;

        /** Which Name.ini file to load Config variables out of */
        public FName ClassConfigName;

        /** The class default object; used for delta serialization and object initialization */
        public FPackageIndex ClassDefaultObject;

        /** Map of all functions by name contained in this class */
        public Dictionary<FName, FPackageIndex /*UFunction*/> FuncMap;

        /**
         * The list of interfaces which this class implements, along with the pointer property that is located at the offset of the interface's vtable.
         * If the interface class isn't native, the property will be null.
         */
        public FImplementedInterface[] Interfaces;

        public override void Deserialize(FAssetArchive Ar, long validPos)
        {
            base.Deserialize(Ar, validPos);

            // serialize the function map
            FuncMap = new Dictionary<FName, FPackageIndex>();
            var funcMapNum = Ar.Read<int>();
            for (var i = 0; i < funcMapNum; i++)
            {
                FuncMap[Ar.ReadFName()] = new FPackageIndex(Ar);
            }

            // Class flags first.
            ClassFlags = Ar.Read<uint>();

            // Variables.
            ClassWithin = new FPackageIndex(Ar);
            ClassConfigName = Ar.ReadFName();

            ClassGeneratedBy = new FPackageIndex(Ar);

            // Load serialized interface classes
            Interfaces = Ar.ReadArray(() => new FImplementedInterface(Ar));

            var bDeprecatedForceScriptOrder = Ar.ReadBoolean();
            var dummy = Ar.ReadFName();

            if (Ar.Ver >= (UE4Version) VER_UE4_ADD_COOKED_TO_UCLASS)
            {
                bCooked = Ar.ReadBoolean();
            }

            // Defaults.
            ClassDefaultObject = new FPackageIndex(Ar);
        }
        
        public Assets.Exports.UObject? ConstructObject()
        {
            var type = ObjectTypeRegistry.Get(Name);
            if (type != null)
            {
                try
                {
                    var instance = Activator.CreateInstance(type);
                    if (instance is Assets.Exports.UObject obj)
                    {
                        return obj;
                    }
                    else
                    {
                        Log.Warning("Class {Type} did have a valid constructor but does not inherit UObject", type);
                    }
                }
                catch (Exception e)
                {
                    Log.Warning(e, "Class {Type} could not be constructed", type);
                }
            }

            return null;
        }

        protected internal override void WriteJson(JsonWriter writer, JsonSerializer serializer)
        {
            base.WriteJson(writer, serializer);

            if (FuncMap is { Count: > 0 })
            {
                writer.WritePropertyName("FuncMap");
                serializer.Serialize(writer, FuncMap);
            }

            if (ClassFlags != 0)
            {
                writer.WritePropertyName("ClassFlags");
                serializer.Serialize(writer, ClassFlags);
            }

            if (ClassWithin is { IsNull: false })
            {
                writer.WritePropertyName("ClassWithin");
                serializer.Serialize(writer, ClassWithin);
            }

            if (!ClassConfigName.IsNone)
            {
                writer.WritePropertyName("ClassConfigName");
                serializer.Serialize(writer, ClassConfigName);
            }

            if (ClassGeneratedBy is { IsNull: false })
            {
                writer.WritePropertyName("ClassGeneratedBy");
                serializer.Serialize(writer, ClassGeneratedBy);
            }

            if (Interfaces is { Length: > 0 })
            {
                writer.WritePropertyName("Interfaces");
                serializer.Serialize(writer, Interfaces);
            }

            if (bCooked)
            {
                writer.WritePropertyName("bCooked");
                serializer.Serialize(writer, bCooked);
            }

            if (ClassDefaultObject is { IsNull: false })
            {
                writer.WritePropertyName("ClassDefaultObject");
                serializer.Serialize(writer, ClassDefaultObject);
            }
        }

        public class FImplementedInterface
        {
            /** the interface class */
            public FPackageIndex Class;

            /** the pointer offset of the interface's vtable */
            public int PointerOffset;

            /** whether or not this interface has been implemented via K2 */
            public bool bImplementedByK2;

            public FImplementedInterface(FAssetArchive Ar)
            {
                Class = new FPackageIndex(Ar);
                PointerOffset = Ar.Read<int>();
                bImplementedByK2 = Ar.ReadBoolean();
            }
        }
    }
}