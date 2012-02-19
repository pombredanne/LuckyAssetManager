﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Reflection;
using Lucky.AssetManager.Processors;

namespace Lucky.AssetManager.Configuration {
    public interface IAssetManagerSettings {
        bool Debug { get; }
        AssetConfiguration Css { get; }
        AssetConfiguration Javascript { get; }
    }

    public class AssetManagerSettings : ConfigurationSection, IAssetManagerSettings {
        [ConfigurationProperty("debug", DefaultValue = false, IsRequired = false)]
        public bool Debug {
            get { return (bool) this["debug"]; }
        }

        [ConfigurationProperty("css")]
        public AssetConfiguration Css {
            get { return (AssetConfiguration) this["css"]; }
        }

        [ConfigurationProperty("javascript")]
        public AssetConfiguration Javascript {
            get { return (AssetConfiguration)this["javascript"]; }
        }
    }

    public interface IAssetConfiguration {
        IEnumerable<IProcessor> Processors { get; }

        [ConfigurationProperty("processors")]
        AssetProcessors ProcessorsRaw { get; }

        [ConfigurationProperty("alternateName", DefaultValue = null, IsRequired = false)]
        string AlternateName { get; }
    }

    public class AssetConfiguration : ConfigurationElement, IAssetConfiguration {

        private IEnumerable<IProcessor> _processors;
        public IEnumerable<IProcessor> Processors {
            get {
                if (_processors == null) {
                    var result = new List<IProcessor>();
                    foreach (var processorObject in ProcessorsRaw) {
                        var processorData = (AssetProcessor) processorObject;
                        var type = processorData.Type.Split(',');
                        var assembly = Assembly.GetAssembly(typeof (IProcessor));
                        if (type.Length > 2) {
                            throw new ConfigurationErrorsException("Malformed asset manager processor type. Format the string like 'Namespace.ClassName, AssemblyName'.");
                        }
                        if (type.Length == 2) {
                            assembly = Assembly.Load(type[1]);
                        }
                        object o = assembly.CreateInstance(type[0]);
                        if (o is IProcessor) {
                            result.Add(o as IProcessor);
                        }
                    }
                    _processors = result;
                }
                return _processors;
            }
        }

        [ConfigurationProperty("processors")]
        public AssetProcessors ProcessorsRaw {
            get {
                return this["processors"] as AssetProcessors;
            }
        }

        [ConfigurationProperty("alternateName", DefaultValue = null, IsRequired = false)]
        public virtual string AlternateName {
            get { return (string) this["alternateName"]; }
        }
    }

    public class AssetProcessors : ConfigurationElementCollection {
        protected override ConfigurationElement CreateNewElement() {
            return new AssetProcessor();
        }

        protected override object GetElementKey(ConfigurationElement element) {
            return ((AssetProcessor) element).Name;
        }
    }

    public class AssetProcessor : ConfigurationElement {
        [ConfigurationProperty("name", IsRequired = true)]
        public string Name {
            get {
                return this["name"] as string;
            }
        }

        [ConfigurationProperty("type", IsRequired = false)]
        public string Type {
            get {
                return this["type"] as string;
            }
        }
    }
}