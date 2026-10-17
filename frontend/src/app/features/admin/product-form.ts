import { Component, OnInit, computed, inject, input, signal } from '@angular/core';
import { rxResource } from '@angular/core/rxjs-interop';
import { FormArray, NonNullableFormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { ProductDetails, ProductUpsert } from '../../core/models/catalog.models';
import { AdminCatalogService } from '../../core/services/admin-catalog.service';
import { CatalogService } from '../../core/services/catalog.service';
import { NotificationService } from '../../core/services/notification.service';
import { apiErrorMessage } from '../../shared/utils/api-error';

/** Create and edit form for a product, including its gallery images and specifications. */
@Component({
  selector: 'app-product-form',
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './product-form.html',
  styleUrl: './product-form.scss',
})
export class ProductForm implements OnInit {
  private readonly fb = inject(NonNullableFormBuilder);
  private readonly adminCatalog = inject(AdminCatalogService);
  private readonly catalog = inject(CatalogService);
  private readonly notifications = inject(NotificationService);
  private readonly router = inject(Router);

  /** Route parameter; undefined on /admin/products/new. */
  readonly id = input<string>();
  protected readonly isNew = computed(() => !this.id());

  protected readonly brands = rxResource({ stream: () => this.catalog.getBrands() });
  protected readonly categories = rxResource({ stream: () => this.catalog.getCategories() });
  protected readonly loadError = signal<string | null>(null);
  protected readonly saving = signal(false);
  protected readonly error = signal<string | null>(null);

  protected readonly form = this.fb.group({
    sku: ['', [Validators.required, Validators.minLength(3), Validators.maxLength(40), Validators.pattern(/^[A-Za-z0-9-]+$/)]],
    name: ['', [Validators.required, Validators.minLength(2), Validators.maxLength(200)]],
    description: ['', Validators.maxLength(4000)],
    price: [0, [Validators.required, Validators.min(0.01)]],
    stockQuantity: [0, [Validators.required, Validators.min(0)]],
    isActive: [true],
    brandId: [0, Validators.min(1)],
    categoryId: [0, Validators.min(1)],
    images: this.fb.array([this.imageGroup()]),
    specifications: this.fb.array([this.specGroup()]),
  });

  protected get images(): FormArray {
    return this.form.controls.images;
  }

  protected get specifications(): FormArray {
    return this.form.controls.specifications;
  }

  ngOnInit(): void {
    const id = this.id();
    if (!id) return;
    this.adminCatalog.getProduct(Number(id)).subscribe({
      next: (product) => this.fill(product),
      error: (error) => this.loadError.set(apiErrorMessage(error)),
    });
  }

  protected addImage(): void {
    this.images.push(this.imageGroup());
  }

  protected addSpec(): void {
    this.specifications.push(this.specGroup());
  }

  protected save(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      this.error.set('Please fix the highlighted fields.');
      return;
    }

    const value = this.form.getRawValue();
    const product: ProductUpsert = {
      ...value,
      images: value.images.filter((i) => i.url.trim()).map((i) => ({ url: i.url, altText: i.altText || null })),
      specifications: value.specifications.filter((s) => s.name.trim() && s.value.trim()),
    };

    const id = this.id();
    const request = id ? this.adminCatalog.updateProduct(Number(id), product) : this.adminCatalog.createProduct(product);
    this.saving.set(true);
    this.error.set(null);
    request.subscribe({
      next: (saved) => {
        this.notifications.success(`${saved.name} was saved.`);
        this.router.navigateByUrl('/admin/products');
      },
      error: (error) => {
        this.saving.set(false);
        this.error.set(apiErrorMessage(error));
      },
    });
  }

  private fill(product: ProductDetails): void {
    this.images.clear();
    product.images.forEach((image) => this.images.push(this.imageGroup(image.url, image.altText)));
    if (!product.images.length) this.addImage();

    this.specifications.clear();
    product.specifications.forEach((spec) => this.specifications.push(this.specGroup(spec.name, spec.value)));
    if (!product.specifications.length) this.addSpec();

    this.form.patchValue({
      sku: product.sku,
      name: product.name,
      description: product.description,
      price: product.price,
      stockQuantity: product.stockQuantity,
      isActive: product.isActive,
      brandId: product.brand.id,
      categoryId: product.category.id,
    });
  }

  private imageGroup(url = '', altText = '') {
    return this.fb.group({ url: [url, Validators.maxLength(500)], altText: [altText, Validators.maxLength(200)] });
  }

  private specGroup(name = '', value = '') {
    return this.fb.group({ name: [name, Validators.maxLength(100)], value: [value, Validators.maxLength(300)] });
  }
}
