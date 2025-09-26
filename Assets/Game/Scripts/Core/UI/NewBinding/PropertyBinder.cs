using System;
using System.ComponentModel;
using System.Linq.Expressions;

namespace Framework.UI
{
    public class PropertyBinder<TProperty> : IDisposable
    {
        private readonly ViewModelBase _viewModel;
        private readonly string _propertyName;
        private readonly Action<TProperty> _updateAction;

        public PropertyBinder(ViewModelBase viewModel, Expression<Func<TProperty>> propertyExpression, Action<TProperty> updateAction)
        {
            _viewModel = viewModel;
            _updateAction = updateAction;

            var memberExpression = propertyExpression.Body as MemberExpression;
            if (memberExpression == null)
                throw new ArgumentException("propertyExpression must be a MemberExpression");
            _propertyName = memberExpression.Member.Name;

            _viewModel.PropertyChanged += OnPropertyChanged;
            Update();
        }

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == _propertyName)
            {
                Update();
            }
        }

        private void Update()
        {
            var prop = _viewModel.GetType().GetProperty(_propertyName);
            if (prop != null)
            {
                _updateAction((TProperty)prop.GetValue(_viewModel));
            }
        }

        public void Dispose()
        {
            if (_viewModel != null)
            {
                _viewModel.PropertyChanged -= OnPropertyChanged;
            }
        }
    }
}