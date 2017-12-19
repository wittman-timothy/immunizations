﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace ConsoleApp1.ILCDCService {
    
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ServiceModel.ServiceContractAttribute(Namespace="http://HL7_ICARE", ConfigurationName="ILCDCService.HL7Exchange")]
    public interface HL7Exchange {
        
        [System.ServiceModel.OperationContractAttribute(Action="http://HL7_ICARE/HL7Exchange/HL7Request", ReplyAction="http://HL7_ICARE/HL7Exchange/HL7RequestResponse")]
        string HL7Request(string username, string password, string HL7Message);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://HL7_ICARE/HL7Exchange/HL7Request", ReplyAction="http://HL7_ICARE/HL7Exchange/HL7RequestResponse")]
        System.Threading.Tasks.Task<string> HL7RequestAsync(string username, string password, string HL7Message);
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public interface HL7ExchangeChannel : ConsoleApp1.ILCDCService.HL7Exchange, System.ServiceModel.IClientChannel {
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public partial class HL7ExchangeClient : System.ServiceModel.ClientBase<ConsoleApp1.ILCDCService.HL7Exchange>, ConsoleApp1.ILCDCService.HL7Exchange {
        
        public HL7ExchangeClient() {
        }
        
        public HL7ExchangeClient(string endpointConfigurationName) : 
                base(endpointConfigurationName) {
        }
        
        public HL7ExchangeClient(string endpointConfigurationName, string remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }
        
        public HL7ExchangeClient(string endpointConfigurationName, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }
        
        public HL7ExchangeClient(System.ServiceModel.Channels.Binding binding, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(binding, remoteAddress) {
        }
        
        public string HL7Request(string username, string password, string HL7Message) {
            return base.Channel.HL7Request(username, password, HL7Message);
        }
        
        public System.Threading.Tasks.Task<string> HL7RequestAsync(string username, string password, string HL7Message) {
            return base.Channel.HL7RequestAsync(username, password, HL7Message);
        }
    }
}