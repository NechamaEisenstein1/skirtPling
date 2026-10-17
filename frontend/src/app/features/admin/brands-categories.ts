import { Component, inject, signal } from '@angular/core';
import { rxResource } from '@angular/core/rxjs-interop';
import { FormsModule } from '@angular/forms';
import { Observable } from 'rxjs';
import { Brand, Category } from '../../core/models/catalog.models';
import { AdminCatalogService } from '../../core/services/admin-catalog.service';
import { CatalogService } from '../../core/services/catalog.service';
import { ConfirmService } from '../../core/services/confirm.service';
import { NotificationService } from '../../core/services/notification.service';
import { apiErrorMessage } from '../../shared/utils/api-error';

/** Brands and categories are small lookup lists, so both are managed inline on one page. */
@Component({
  selector: 'app-brands-categories',
  imports: [FormsModule],
  templateUrl: './brands-categories.html',
  styles: `
    .panels { display: grid; grid-template-columns: 1fr 1fr; gap: 24px; align-items: start; }
    .item { display: flex; justify-content: space-between; align-items: center; gap: 8px; padding: 10px 0; border-bottom: 1px solid var(--line); }
    .editor { display: flex; flex-direction: column; gap: 8px; margin-bottom: 16px; }
    .editor .form-actions { margin-top: 0; }
    .description { font-size: .85rem; }
    @media (max-width: 900px) { .panels { grid-template-columns: 1fr; } }
  `,
})
export class BrandsCategories {
  private readonly catalog = inject(CatalogService);
  private readonly adminCatalog = inject(AdminCatalogService);
  private readonly notifications = inject(NotificationService);
  private readonly confirm = inject(ConfirmService);

  protected readonly brands = rxResource({ stream: () => this.catalog.getBrands() });
  protected readonly categories = rxResource({ stream: () => this.catalog.getCategories() });

  /** The brand/category being edited; id 0 means a new one. */
  protected readonly brandDraft = signal<{ id: number; name: string; description: string } | null>(null);
  protected readonly categoryDraft = signal<{ id: number; name: string } | null>(null);

  protected editBrand(brand?: Brand): void {
    this.brandDraft.set(brand ? { ...brand } : { id: 0, name: '', description: '' });
  }

  protected editCategory(category?: Category): void {
    this.categoryDraft.set(category ? { ...category } : { id: 0, name: '' });
  }

  protected saveBrand(): void {
    const draft = this.brandDraft();
    if (!draft) return;
    this.run(this.adminCatalog.saveBrand({ name: draft.name, description: draft.description }, draft.id || undefined), 'Brand saved.', () => {
      this.brandDraft.set(null);
      this.brands.reload();
    });
  }

  protected saveCategory(): void {
    const draft = this.categoryDraft();
    if (!draft) return;
    this.run(this.adminCatalog.saveCategory({ name: draft.name }, draft.id || undefined), 'Category saved.', () => {
      this.categoryDraft.set(null);
      this.categories.reload();
    });
  }

  protected async deleteBrand(brand: Brand): Promise<void> {
    if (!(await this.confirm.ask('Delete brand?', `“${brand.name}” will be removed. Brands that still have products cannot be deleted.`))) return;
    this.run(this.adminCatalog.deleteBrand(brand.id), 'Brand deleted.', () => this.brands.reload());
  }

  protected async deleteCategory(category: Category): Promise<void> {
    if (!(await this.confirm.ask('Delete category?', `“${category.name}” will be removed. Categories that still have products cannot be deleted.`))) return;
    this.run(this.adminCatalog.deleteCategory(category.id), 'Category deleted.', () => this.categories.reload());
  }

  private run(request: Observable<unknown>, successMessage: string, onSuccess: () => void): void {
    request.subscribe({
      next: () => {
        this.notifications.success(successMessage);
        onSuccess();
      },
      error: (error) => this.notifications.error(apiErrorMessage(error)),
    });
  }
}
