using NLua.Method;
using System;
using System.Collections.Generic;
using System.Text;

namespace CaveGame.Common.LuaInterop
{

    public class LuaBindingInstance<TArgs> where TArgs : LuaEventArgs
    {
        protected LuaEvent<TArgs> Handler;
        public NLua.LuaFunction BindFunction;

        public LuaBindingInstance(LuaEvent<TArgs> handler, NLua.LuaFunction func)
        {
            Handler = handler;
            BindFunction = func;
        }

        public virtual void Invoke(TArgs args) => BindFunction.Call(args);
        
        public void Unbind()
        {
            Handler.Unbind(this);
            BindFunction.Dispose();
        }
    
    }

    public class LuaEventArgs
    {
        public bool Cancelled { get; private set; }
        public void SetCancelled(bool cancelled)=>Cancelled = cancelled;
    
    }

    public class LuaEvent<TArgs> where TArgs : LuaEventArgs
    {

        List<LuaBindingInstance<TArgs>> Bindings = new List<LuaBindingInstance<TArgs>>();

        public event Action<TArgs> Monitor;

        public virtual LuaBindingInstance<TArgs> Bind(NLua.LuaFunction binding)
        {
            LuaBindingInstance<TArgs> instance = new LuaBindingInstance<TArgs>(this, binding);
            Bindings.Add(instance);
            return instance;
        }

        public virtual bool Invoke(TArgs funcArgs)
        {
            Monitor?.Invoke(funcArgs);
            foreach(LuaBindingInstance<TArgs> instance in Bindings)
            {
                instance.Invoke(funcArgs);
                if (funcArgs != null && funcArgs.Cancelled)
                    return false;
            }
            return true;
        }

        public virtual void Unbind(LuaBindingInstance<TArgs> instance) => Bindings.Remove(instance);
    }
}
