import { Component, OnDestroy, OnInit, ViewChild } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { NgxGalleryAnimation, NgxGalleryImage, NgxGalleryOptions } from '@kolkov/ngx-gallery';
import { TabDirective, TabsetComponent } from 'ngx-bootstrap/tabs';
import { take } from 'rxjs/operators';
import { Member } from 'src/app/_models/member';
import { Message } from 'src/app/_models/message';
import { User } from 'src/app/_models/user';
import { AccountService } from 'src/app/_services/account.service';
import { MembersService } from 'src/app/_services/members.service';
import { MessageService } from 'src/app/_services/message.service';
import { PresenceService } from 'src/app/_services/presence.service';

@Component({
  selector: 'app-member-detail',
  templateUrl: './member-detail.component.html',
  styleUrls: ['./member-detail.component.css']
})
export class MemberDetailComponent implements OnInit, OnDestroy {

  @ViewChild('memberTabs', {
    static: true
  }) memberTabs: TabsetComponent;
  member: Member;
  galleryOptions: NgxGalleryOptions[];
  galleryImages: NgxGalleryImage[];
  activeTab: TabDirective;
  messages: Message[] = [];
  user: User;

  constructor(private memberService: MembersService, private route: ActivatedRoute,
    private messageService: MessageService, public presence: PresenceService,
    private accountservice: AccountService,
    private router: Router) {
    this.accountservice.currentUser$.pipe(take(1)).subscribe(user => {
      this.user = user;
      this.router.routeReuseStrategy.shouldReuseRoute = ()=>false;
    });
  }

  ngOnInit(): void {
    this.route.data.subscribe(data => {
      this.member = data.member;

    })


    //this.loadMember();
    this.galleryOptions = [{
      width: '500px',
      height: '500px',
      imagePercent: 100,
      thumbnailsColumns: 4,
      imageAnimation: NgxGalleryAnimation.Slide,
      preview: false
    }]
    this.galleryImages = this.getImages();
    this.route.queryParams.subscribe(params => {
      console.log(params.tab);
      params.tab ? this.selectTab(params.tab) : this.selectTab(0);
    })
  }



  getImages(): NgxGalleryImage[] {
    const imageUrls = [];
    for (const photo of this.member.photos) {
      imageUrls.push({
        small: photo?.url,
        medium: photo?.url,
        big: photo?.url
      });
    }
    return imageUrls;
  }


  onTabActivated(data: TabDirective) {
    this.activeTab = data;

    if (this.activeTab.heading === 'Messages'
      && this.messages.length === 0) {
      this.messageService.createHubConnection(this.user, this.member.userName);
    } else {
      this.messageService.stopHubConnection();
    }


  }

  selectTab(tabId: number) {
    tabId = Number(tabId);
    this.memberTabs.tabs[tabId].active = true;
  }

  loadMessages() {
    this.messageService.getMessageThread(this.member.userName)
      .subscribe(messages => {
        console.log(messages);
        this.messages = messages;
      });
  }

  ngOnDestroy(): void {
    this.messageService.stopHubConnection();
  }


}
