import { HttpClient, HttpParams, HttpResponse } from '@angular/common/http';
import { inject, Injectable, signal } from '@angular/core';
import { environment } from '../../environments/environment';
import { Member } from '../_models/member';
import { of, tap } from 'rxjs';
import { Photo } from '../_models/photo';
import { PaginatedResult } from '../_models/pagination';
import { MemberParameters } from '../_models/memberParameters';
import { AccountService } from './account.service';

@Injectable({
  providedIn: 'root'
})
export class MembersService {
  private http = inject(HttpClient)
  private accountService = inject(AccountService);
  user = this.accountService.currentUser();
  baseUrl = environment.apiUrl;
  paginatedResults = signal<PaginatedResult<Member[]> | null>(null);
  memberParams = signal<MemberParameters>(new MemberParameters(this.user));
  memberCache = new Map();

  resetUserParams() {
    this.memberParams.set(new MemberParameters(this.user));
  }

  getMembers() {
    const response = this.memberCache.get(Object.values(this.memberParams()).join('-'));
    if (response) {
      return this.setPaginatedResults(response);
    }

    let params = this.setPaginationHeaders(this.memberParams().pageNumber, this.memberParams().pageSize);
    params = params.append('gender', this.memberParams().gender)
    params = params.append('minAge', this.memberParams().minAge);
    params = params.append('maxAge', this.memberParams().maxAge);
    params = params.append('orderBy', this.memberParams().orderBy);

    return this.http.get<Member[]>(this.baseUrl + 'users', { observe: 'response', params }).subscribe({
      next: response => {
        this.setPaginatedResults(response);
        this.memberCache.set(Object.values(this.memberParams()).join('-'), response);
      }
    })
  }

  private setPaginatedResults(response: HttpResponse<Member[]>) {
    this.paginatedResults.set({
      items: response.body as Member[],
      pagination: JSON.parse(response.headers.get('Pagination')!)
    })
  }

  private setPaginationHeaders(pageNumber: number, pageSize: number) {
    let params = new HttpParams();

    if (pageNumber && pageSize) {
      params = params.append('pageNumber', pageNumber);
      params = params.append('pageSize', pageSize);
    }

    return params;
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
