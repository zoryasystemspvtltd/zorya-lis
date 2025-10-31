import { Component, OnInit, ViewChild } from '@angular/core';
import { ListModuleComponent } from '../../_components';

@Component({
  selector: 'app-equipment',
  template: '<br><app-his-sample (onGetOrder)="refreshOrder($event)"></app-his-sample><br><app-list-module [schemma]="moduleJson"></app-list-module>'
})

export class ListRawSampleComponent implements OnInit {
  @ViewChild(ListModuleComponent) chield : ListModuleComponent;
  constructor() { }

  ngOnInit() {
  }
  public moduleJson: any = {
    url: 'samples',
    heading: 'List of sample',
    module: 'Patients',
    hideAction: false,
    allowPrint:true,
    hideSearch: false,
    hideCreate: true,
    allowPaging: true,
    //filterStatus:0,
    allowedFilter: [0, 1, 2],
    auto_refresh:true,
    isNew:true,
    elements: [
      {
        heading: 'Sample No', fieldName: 'reF_VISITNO', sortable: true, width: '12%', type: 'link'
      },
      {
        heading: 'Admission No', fieldName: 'admissionno', sortable: false, width: '10%', type: 'label'
      },
      {
        heading: 'Test Name', fieldName: 'testproF_CODE', sortable: false, width: '10%', type: 'label'
      },
      {
        heading: 'Param Name', fieldName: 'paramcode', sortable: false, width: '16%', type: 'label'
      },
      {
        heading: 'Collection Date', fieldName: 'createdAt', sortable: true, width: '16%', type: 'date', format: 'dd/MM/yyyy'
      },
      {
        heading: 'Patient Name', fieldName: 'patfname', sortable: false, width: '20%', type: 'label'
      }
    ]
  }

  refreshOrder(event) {
    setTimeout(() => this.chield.getItems(),2000);
  }
}
