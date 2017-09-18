using System;
using System.Collections.Generic;
using System.Linq;

using TaskTracker.Common;
using TaskTracker.Model;

namespace TaskTracker.Client.WPF.ViewModels
{
    public class StageTreeViewModel : SelectionItemViewModel
    {
        private bool isExpanded;

        public StageTreeViewModel(Stage stage, bool isExpanded = false)
        {
            ArgumentValidation.ThrowIfNull(stage, nameof(stage));

            this.isExpanded = isExpanded;
            this.Stage = stage;
            this.ChildItems = stage.SubStages.Select(s => new StageTreeViewModel(s));
        }

        public Stage Stage { get; private set; }

        public bool IsExpanded
        {
            get { return isExpanded; }
            set
            {
                if (isExpanded != value)
                {
                    isExpanded = value;
                    NotifyPropertyChanged(nameof(IsExpanded));
                }
            }
        }

        public string ItemName { get { return Stage.Name; } }  
        
        public string SelectedItemName
        {
            get
            {
                return $"Stage[Id: {Stage.Id}; Level: {Stage.Level}; Name: {Stage.Name}]";
            }
        }

        public IEnumerable<StageTreeViewModel> ChildItems { get; private set; }       
    }    
}
