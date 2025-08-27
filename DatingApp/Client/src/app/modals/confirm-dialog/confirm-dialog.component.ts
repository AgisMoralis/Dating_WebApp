import { Component, inject } from '@angular/core';
import { BsModalRef } from 'ngx-bootstrap/modal';

@Component({
  selector: 'app-confirm-dialog',
  standalone: true,
  imports: [],
  templateUrl: './confirm-dialog.component.html',
  styleUrl: './confirm-dialog.component.css'
})
export class ConfirmDialogComponent {
  bsModalRef = inject(BsModalRef);
  title = '';
  message = '';
  btnOkText = '';
  btnCancelText = '';

  // We set this flag to true only if the user clicks the confirmation button
  // otherwise it remains false (in case he clicks somewhere else or the cancel button)
  confirmButtonPressed = false;

  confirm(){
    this.confirmButtonPressed = true;
    this.bsModalRef.hide();
  }

  decline(){
    this.bsModalRef.hide();
  }
}
