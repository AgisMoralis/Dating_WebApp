import { HttpClient, HttpResponse } from '@angular/common/http';
import { inject, Injectable, signal } from '@angular/core';
import { environment } from '../../environments/environment';
import { Member } from '../_models/member';
import { of, tap } from 'rxjs';
import { Photo } from '../_models/photo';
import { PaginatedResult } from '../_models/pagination';
import { MemberParameters } from '../_models/memberParameters';
import { AccountService } from './account.service';
import { setPaginatedResponse, setPaginationHeaders } from './paginationHelper';

@Injectable({
  providedIn: 'root'
})
export class MembersService {
  private http = inject(HttpClient)
  private accountService = inject(AccountService);
  user = this.accountService.currentUser();
  baseUrl = environment.apiUrl;
  paginatedResult = signal<PaginatedResult<Member[]> | null>(null);
  memberParams = signal<MemberParameters>(new MemberParameters(this.user));
  memberCache = new Map();

  resetUserParams() {
    this.memberParams.set(new MemberParameters(this.user));
  }

  getMembers() {
    const response = this.memberCache.get(Object.values(this.memberParams()).join('-'));
    if (response) {
      return setPaginatedResponse(response, this.paginatedResult);
    }

    let params = setPaginationHeaders(this.memberParams().pageNumber, this.memberParams().pageSize);
    params = params.append('gender', this.memberParams().gender)
    params = params.append('minAge', this.memberParams().minAge);
    params = params.append('maxAge', this.memberParams().maxAge);
    params = params.append('orderBy', this.memberParams().orderBy);

    return this.http.get<Member[]>(this.baseUrl + 'users', { observe: 'response', params }).subscribe({
      next: response => {
        setPaginatedResponse(response, this.paginatedResult);
        this.memberCache.set(Object.values(this.memberParams()).join('-'), response);
      }
    })
  }

  getMember(username: string) {
    const member: Member = [...this.memberCache.values()]
      .reduce((arr, elem: HttpResponse<Member[]>) => arr.concat(elem.body), [])
      .find((m: Member) => m.username === username);
    
    if(member) {
      return of(member);
    }

    return this.http.get<Member>(this.baseUrl + 'users/' + username);
  }

  updateMember(member: Member) {
    return this.http.put(this.baseUrl + 'users', member);
  }

  setMainPhoto(photo: Photo) {
    return this.http.put(this.baseUrl + 'users/set-main-photo/' + photo.id, {});
  }

  deletePhoto(photo: Photo) {
    return this.http.delete(this.baseUrl + 'users/delete-photo/' + photo.id);
  };
}
