import { Component, OnInit } from '@angular/core';
import { Pagination } from '../_models/pagination';
import { Message } from '../_models/message';
import { MessageService } from '../_services/message.service';
import { ConfirmService } from '../_services/confirm.service';

@Component({
  selector: 'app-messages',
  templateUrl: './messages.component.html',
  styleUrls: ['./messages.component.css']
})
export class MessagesComponent implements OnInit {

  messages: Message[] = [];
  pagination: Pagination;
  container: string = 'Unread';
  pageSize = 5;
  pageNumber = 1;
  loading = false;
  constructor(private messageService: MessageService,
    private confirmService: ConfirmService,) { }

  ngOnInit(): void {
    this.loadMessages();
  }



  loadMessages() {
    this.loading = true;
    this.messageService.getMessages(this.pageNumber, this.pageSize, this.container)
      .subscribe(
        response => {
          console.log(response.result);
          this.messages = response.result;
          this.pagination = response.pagination;
          this.loading = false;
        }
      );

  }

  pageChanged(event: any) {
    this.pageNumber = event.page;
    this.loadMessages();
  }


  deleteMessage(id: number) {
    this.confirmService.confirm('confirm delete message',
      'This cannot be undo').subscribe(result => {
        if (result) {
          this.messageService.deleteMessage(id).subscribe(message => {
            this.messages.splice(this.messages.findIndex(m => m.id === id), 1);
          });
        }
      });
  }


}
