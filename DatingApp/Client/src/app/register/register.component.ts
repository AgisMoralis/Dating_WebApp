import { Component, input, output, inject, OnInit } from '@angular/core';
import { AbstractControl, FormBuilder, FormControl, FormGroup, ReactiveFormsModule, ValidatorFn, Validators } from '@angular/forms';
import { AccountService } from '../_services/account.service';
import { ToastrService } from 'ngx-toastr';
import { TextInputComponent } from "../_forms/text-input/text-input.component";
import { DatePickerComponent } from '../_forms/date-picker/date-picker.component';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [ReactiveFormsModule, TextInputComponent, DatePickerComponent],
  templateUrl: './register.component.html',
  styleUrl: './register.component.css'
})
export class RegisterComponent implements OnInit {
  private toastr = inject(ToastrService);
  private accountService = inject(AccountService);
  private formBuilder = inject(FormBuilder);
  cancelRegister = output<boolean>();
  model: any = {};
  registerForm: FormGroup = new FormGroup({});
  maxDate = new Date();

  ngOnInit() {
    this.initializeForm();
  }

  initializeForm() {
    this.registerForm = this.formBuilder.group({
      gender: ['male'],
      knownAs: ['', Validators.required, Validators.maxLength(30)],
      username: ['', Validators.required, Validators.maxLength(30)],
      password: ['', [Validators.required, Validators.minLength(6), Validators.maxLength(20)]],
      confirmPassword: ['', [Validators.required, this.matchValues('password')]],
      dateOfBirth: ['', Validators.required],
      city: ['', Validators.required],
      country: ['', Validators.required]
    });
    this.maxDate.setFullYear(this.maxDate.getFullYear() - 18);
    this.registerForm.controls['password'].valueChanges.subscribe({
      next: () => {this.registerForm.controls['confirmPassword'].updateValueAndValidity();}
    });
  }

  matchValues(matchTo: string): ValidatorFn {
    return (control: AbstractControl) => {
      return control.value === control.parent?.get(matchTo)?.value ? null : { isMatching: true };
    };

  }

  register() {
    this.accountService.register(this.model).subscribe({
      next: response => {
        console.log(response);
        this.cancel();
      },
      error: error => this.toastr.error(error.error)
    })
  }

  cancel() {
    this.cancelRegister.emit(false);
  }
}
