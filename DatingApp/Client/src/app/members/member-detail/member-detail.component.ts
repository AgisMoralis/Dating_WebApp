import { Component, inject, OnDestroy, OnInit, ViewChild } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { Member } from '../../_models/member';
import { TabDirective, TabsetComponent, TabsModule } from 'ngx-bootstrap/tabs';
import { GalleryModule, GalleryItem, ImageItem } from 'ng-gallery';
import { TimeagoModule } from 'ngx-timeago';
import { DatePipe } from '@angular/common';
import { MemberMessagesComponent } from "../member-messages/member-messages.component";
import { MessageService } from '../../_services/message.service';
import { PresenceService } from '../../_services/presence.service';
import { AccountService } from '../../_services/account.service';
import { HubConnectionState } from '@microsoft/signalr';

@Component({
  selector: 'app-member-detail',
  standalone: true,
  imports: [TabsModule, GalleryModule, TimeagoModule, DatePipe, MemberMessagesComponent],
  templateUrl: './member-detail.component.html',
  styleUrl: './member-detail.component.css'
})
export class MemberDetailComponent implements OnInit, OnDestroy {
  private messageService = inject(MessageService)
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private accountService = inject(AccountService);
  presenceService = inject(PresenceService);
  member: Member = {} as Member;
  images: GalleryItem[] = [];
  @ViewChild('memberTabs', {static: true}) memberTabs?: TabsetComponent
  activeTab?: TabDirective;

  ngOnInit(): void {
   this.route.data.subscribe({
    // Use the "member-detailed" resolver to extract the "member" from the creation of the route
    next: data => {
      this.member = data['member'];
      this.member && this.member.photos.map(p => {
          this.images.push(new ImageItem({
            src: p.url,
            thumb: p.url
          }));
        })
    }
   })

   // "".paramMap" is an Observable that emits a "ParamMap" object whenever the route parameters change.
   // The subscribtion here sets up a listener, that runs the "onRouteParametersChange" every time the route parameters change.
   this.route.paramMap.subscribe({
    next: _ => this.onRouteParametersChange()
   })

   this.route.queryParams.subscribe({
    next: params => {
      params['tab'] && this.selectTab(params['tab'])
    }
   })

  }

  ngOnDestroy(): void {
    this.messageService.stopHubConnection();
  }

  selectTab(heading: string) {
    if(this.memberTabs) {
      const messageTab = this.memberTabs.tabs.find(x => x.heading === heading)
      if (messageTab) {
        messageTab.active = true;
      }
    }
  }

  // This method is called whenever the route parameters change.
  // It is useful to resolve the bug when we navigate to a specific member "Messages" tab
  // and a message notification arrives from another member. In that case, the active
  // message hub connection should be stopped and a new one should be created for the new member.
  onRouteParametersChange() {
    const user = this.accountService.currentUser();
    if(!user) return;
    if(this.messageService.hubConnection?.state === HubConnectionState.Connected && this.activeTab?.heading === 'Messages') {
      this.messageService.hubConnection.stop().then(() => {
        this.messageService.createHubConnection(user, this.member.username);
      });
    }
  }

  onTabActivated(data: TabDirective) {
    this.activeTab = data;
    // Whenever a tab is activated, this code updates the query params in the URL,
    // by using the router's navigate method and adding the tab heading as a query param
    this.router.navigate([], {
      relativeTo: this.route,
      queryParams: { tab: this.activeTab.heading },
      queryParamsHandling: 'merge'
    });

    if (this.activeTab.heading === 'Messages' && this.member) {
        const user = this.accountService.currentUser();
        if(!user) return;
        this.messageService.createHubConnection(user, this.member.username);
    } else {
      this.messageService.stopHubConnection();
    }
  }

}
