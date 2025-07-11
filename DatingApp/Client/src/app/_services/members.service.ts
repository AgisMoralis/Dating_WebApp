import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable, signal } from '@angular/core';
import { environment } from '../../environments/environment';
import { Member } from '../_models/member';
import { of, tap } from 'rxjs';
import { Photo } from '../_models/photo';
import { PaginatedResult } from '../_models/pagination';
import { MemberParameters } from '../_models/memberParameters';

@Injectable({
  providedIn: 'root'
})
export class MembersService {
  private http = inject(HttpClient)
  baseUrl = environment.apiUrl;
  paginatedResults = signal<PaginatedResult<Member[]> | null>(null);

  getMembers(memberParams: MemberParameters) {
    let params = this.setPaginationHeaders(memberParams.pageNumber, memberParams.pageSize);
    params = params.append('gender', memberParams.gender)
    params = params.append('minAge', memberParams.minAge);
    params = params.append('maxAge', memberParams.maxAge);
    params = params.append('orderBy', memberParams.orderBy);

    return this.http.get<Member[]>(this.baseUrl + 'users', {observe: 'response', params}).subscribe({
      next: response => {
        this.paginatedResults.set({
          items: response.body as Member[],
          pagination: JSON.parse(response.headers.get('Pagination')!)
        })
      }
    })
  }

  private setPaginationHeaders(pageNumber: number, pageSize: number) {
    let params = new HttpParams();

    if(pageNumber && pageSize){
      params = params.append('pageNumber', pageNumber);
      params = params.append('pageSize', pageSize);
    }

    return params;
  }

  getMember(username: string) {
    //const member = this.members().find(x => x.username === username);
    //if(member !== undefined){
    //  return of(member);
    //}

    return this.http.get<Member>(this.baseUrl + 'users/' + username);
  }

  updateMember(member: Member) {
    return this.http.put(this.baseUrl + 'users', member).pipe(
      //tap(() => {
      //  this.members.update(members => members.map(m => m.username === member.username ? member : m))
      //})
    )
  }

  setMainPhoto(photo: Photo) {
    return this.http.put(this.baseUrl + 'users/set-main-photo/' + photo.id, {}).pipe(
      //tap(() => {
      //  this.updateMainPhoto(photo.id, photo.url);
      //})
    );
  }

  updateMainPhoto(id: number, url: string){
    //this.members.update(members =>
    //  members.map(m => {
    //    if (m.photos.some(p => p.id === id && p.url === url)){
    //      m.photoUrl = url;
    //    }
    //    return m;
    //  }))
  }

  deletePhoto(photo: Photo) {
    return this.http.delete(this.baseUrl + 'users/delete-photo/' + photo.id).pipe(
      //tap(() => {
      //  this.members.update(members =>
      //    members.map(m => {
      //      if(m.photos.includes(photo)){
      //        m.photos = m.photos.filter(p => p.id !== photo.id);
      //      }
      //      return m;
      //    }))
      //})
    );
  };
}
