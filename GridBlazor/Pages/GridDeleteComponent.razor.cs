﻿using GridBlazor.Columns;
using GridBlazor.Resources;
using GridShared.Columns;
using GridShared.Utility;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GridBlazor.Pages
{
    public partial class GridDeleteComponent<T> : ICustomGridComponent<T>
    {
        private int _sequence = 0;
        private QueryDictionary<RenderFragment> _grids;
        private IEnumerable<string> _tabGroups;

        public string Error { get; set; } = "";

        [CascadingParameter(Name = "GridComponent")]
        protected GridComponent<T> GridComponent { get; set; }

        [Parameter]
        public T Item { get; set; }

        protected override async Task OnParametersSetAsync()
        {
            _grids = new QueryDictionary<RenderFragment>();
            foreach (var column in GridComponent.Grid.Columns)
            {
                if (((ICGridColumn)column).SubGrids != null)
                {
                    var values = ((ICGridColumn)column).GetSubGridKeyValues(Item).Values.ToArray();
                    var grid = await ((ICGridColumn)column).SubGrids(values, false, true, false, true) as ICGrid;
                    _grids.Add(column.Name, CreateSubGridComponent(grid));
                }
            }
            _tabGroups = GridComponent.Grid.Columns.Where(r => !string.IsNullOrWhiteSpace(r.TabGroup))
                .Select(r => r.TabGroup).Distinct();
        }

        private RenderFragment CreateSubGridComponent(ICGrid grid) => builder =>
        {
            Type gridComponentType = typeof(GridComponent<>).MakeGenericType(grid.Type);
            builder.OpenComponent(++_sequence, gridComponentType);
            builder.AddAttribute(++_sequence, "Grid", grid);
            builder.CloseComponent();
        };

        protected async Task DeleteItem()
        {
            try
            {
                await GridComponent.DeleteItem(this);
            }
            catch (Exception)
            {
                Error = Strings.DeleteError;
            }
        }

        protected void BackButtonClicked()
        {
            GridComponent.BackButton();
        }
    }
}