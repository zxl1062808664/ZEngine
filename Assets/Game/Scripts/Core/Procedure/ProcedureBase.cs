using Framework.Core;

namespace Framework.Procedure
{
    public abstract class ProcedureBase
    {
        protected ProcedureModule ProcedureModule { get; private set; }

        internal void SetProcedureModule(ProcedureModule module)
        {
            ProcedureModule = module;
        }

        public abstract void OnEnter(object userData);
        public abstract void OnUpdate(float deltaTime, float realDeltaTime);
        public abstract void OnLeave(object userData);

        public virtual void OnDestroy()
        {
        }
    }
}