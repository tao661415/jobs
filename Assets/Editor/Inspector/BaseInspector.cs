using UnityEditor; 

namespace TGame.Editor.Inspector
{
    /// <summary>
    /// 作者: Teddy
    /// 时间: 2018/03/05
    /// 功能: 基础检视面板类
    /// </summary>
    public class BaseInspector : UnityEditor.Editor
    {
        // 是否绘制基础GUI，默认为true
        protected virtual bool DrawBaseGUI { get { return true; } }

        // 编译状态标识
        private bool isCompiling = false;
        // 在编辑器中更新检视面板方法
        protected virtual void OnInspectorUpdateInEditor() { }

        // 当启用时调用
        private void OnEnable()
        {
            OnInspectorEnable();
            EditorApplication.update += UpdateEditor;
        }
        // 在启用时调用的虚拟方法
        protected virtual void OnInspectorEnable() { }

        // 当禁用时调用
        private void OnDisable()
        {
            EditorApplication.update -= UpdateEditor;
            OnInspectorDisable();
        }
        // 在禁用时调用的虚拟方法
        protected virtual void OnInspectorDisable() { }

        // 更新编辑器方法
        private void UpdateEditor()
        {
            if (!isCompiling && EditorApplication.isCompiling)
            {
                isCompiling = true;
                OnCompileStart();
            }
            else if (isCompiling && !EditorApplication.isCompiling)
            {
                isCompiling = false;
                OnCompileComplete();
            }
            OnInspectorUpdateInEditor();
        }

        // 重写的检视面板绘制方法
        public override void OnInspectorGUI()
        {
            // 如果需要绘制基础GUI，则调用基类的绘制方法
            if (DrawBaseGUI)
            {
                base.OnInspectorGUI();
            }
        }

        // 编译开始时调用的虚拟方法
        protected virtual void OnCompileStart() { }
        // 编译完成时调用的虚拟方法
        protected virtual void OnCompileComplete() { }
    }
}