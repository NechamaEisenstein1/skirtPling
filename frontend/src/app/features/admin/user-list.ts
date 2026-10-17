import { DatePipe } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import { rxResource } from '@angular/core/rxjs-interop';
import { AdminUser } from '../../core/models/admin.models';
import { AdminService } from '../../core/services/admin.service';
import { AuthService } from '../../core/services/auth.service';
import { ConfirmService } from '../../core/services/confirm.service';
import { NotificationService } from '../../core/services/notification.service';
import { Pagination } from '../../shared/components/pagination';
import { apiErrorMessage } from '../../shared/utils/api-error';

@Component({
  selector: 'app-user-list',
  imports: [DatePipe, Pagination],
  template: `
    <h1>Users</h1>
    <form class="toolbar" (submit)="$event.preventDefault(); query.set({ search: term.value.trim() || undefined, page: 1, pageSize: 20 })">
      <input #term type="search" placeholder="Email or name" aria-label="Search users" />
      <button class="btn btn-outline btn-sm" type="submit">Search</button>
    </form>

    @if (users.error()) {
      <div class="state">Couldn't load users. <button class="btn-link" (click)="users.reload()">Retry</button></div>
    } @else if (users.value(); as result) {
      <div class="table-wrap">
        <table>
          <thead><tr><th>User</th><th>Joined</th><th>Roles</th><th>Status</th><th></th></tr></thead>
          <tbody>
            @for (user of result.items; track user.id) {
              <tr>
                <td>{{ user.fullName }}<br /><span class="muted">{{ user.email }}</span></td>
                <td>{{ user.createdAt | date: 'mediumDate' }}</td>
                <td>{{ user.roles.join(', ') }}</td>
                <td><span class="badge" [class.danger]="user.isLocked" [class.success]="!user.isLocked">{{ user.isLocked ? 'Locked' : 'Active' }}</span></td>
                <td>
                  @if (user.id === currentUserId) {
                    <span class="muted">This is you</span>
                  } @else {
                    <button class="btn-link" type="button" (click)="toggleAdmin(user)">{{ isAdmin(user) ? 'Remove admin' : 'Make admin' }}</button>
                    <button class="btn-link" [class.danger]="!user.isLocked" type="button" (click)="toggleLock(user)">{{ user.isLocked ? 'Unlock' : 'Lock' }}</button>
                  }
                </td>
              </tr>
            }
          </tbody>
        </table>
      </div>
      <app-pagination [page]="result.page" [pageSize]="result.pageSize" [totalCount]="result.totalCount"
        (pageChange)="query.update(q => ({ ...q, page: $event }))" />
    } @else {
      <div class="state"><div class="spinner"></div></div>
    }
  `,
})
export class UserList {
  private readonly admin = inject(AdminService);
  private readonly confirm = inject(ConfirmService);
  private readonly notifications = inject(NotificationService);

  protected readonly currentUserId = inject(AuthService).user()?.id;
  protected readonly query = signal<{ search?: string; page: number; pageSize: number }>({ page: 1, pageSize: 20 });
  protected readonly users = rxResource({
    params: () => this.query(),
    stream: ({ params }) => this.admin.searchUsers(params),
  });

  protected isAdmin(user: AdminUser): boolean {
    return user.roles.includes('Admin');
  }

  protected async toggleAdmin(user: AdminUser): Promise<void> {
    const makeAdmin = !this.isAdmin(user);
    const confirmed = await this.confirm.ask(
      makeAdmin ? 'Grant admin access?' : 'Remove admin access?',
      `${user.email} will ${makeAdmin ? 'be able to manage the whole store' : 'lose access to the admin area'}.`,
      makeAdmin ? 'Grant access' : 'Remove access',
    );
    if (confirmed) this.update(user, { isAdmin: makeAdmin, isLocked: user.isLocked });
  }

  protected async toggleLock(user: AdminUser): Promise<void> {
    if (!user.isLocked && !(await this.confirm.ask('Lock this account?', `${user.email} will not be able to sign in.`, 'Lock account'))) return;
    this.update(user, { isAdmin: this.isAdmin(user), isLocked: !user.isLocked });
  }

  private update(user: AdminUser, access: { isAdmin: boolean; isLocked: boolean }): void {
    this.admin.updateUserAccess(user.id, access).subscribe({
      next: () => {
        this.notifications.success(`${user.email} was updated.`);
        this.users.reload();
      },
      error: (error) => this.notifications.error(apiErrorMessage(error)),
    });
  }
}
