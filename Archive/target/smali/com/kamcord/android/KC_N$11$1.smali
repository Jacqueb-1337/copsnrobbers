.class final Lcom/kamcord/android/KC_N$11$1;
.super Ljava/lang/Object;

# interfaces
.implements Ljava/lang/Runnable;


# annotations
.annotation system Ldalvik/annotation/EnclosingMethod;
    value = Lcom/kamcord/android/KC_N$11;->run()V
.end annotation

.annotation system Ldalvik/annotation/InnerClass;
    accessFlags = 0x0
    name = null
.end annotation


# instance fields
.field private synthetic a:Landroid/graphics/Bitmap;

.field private synthetic b:Lcom/kamcord/android/KC_N$11;


# direct methods
.method constructor <init>(Lcom/kamcord/android/KC_N$11;Landroid/graphics/Bitmap;)V
    .locals 0

    iput-object p1, p0, Lcom/kamcord/android/KC_N$11$1;->b:Lcom/kamcord/android/KC_N$11;

    iput-object p2, p0, Lcom/kamcord/android/KC_N$11$1;->a:Landroid/graphics/Bitmap;

    invoke-direct {p0}, Ljava/lang/Object;-><init>()V

    return-void
.end method


# virtual methods
.method public final run()V
    .locals 2

    iget-object v0, p0, Lcom/kamcord/android/KC_N$11$1;->b:Lcom/kamcord/android/KC_N$11;

    iget-object v0, v0, Lcom/kamcord/android/KC_N$11;->a:Lcom/kamcord/android/KC_N;

    iget-object v0, v0, Lcom/kamcord/android/KC_N;->N:Landroid/widget/ImageButton;

    iget-object v1, p0, Lcom/kamcord/android/KC_N$11$1;->a:Landroid/graphics/Bitmap;

    invoke-virtual {v0, v1}, Landroid/widget/ImageButton;->setImageBitmap(Landroid/graphics/Bitmap;)V

    iget-object v0, p0, Lcom/kamcord/android/KC_N$11$1;->b:Lcom/kamcord/android/KC_N$11;

    iget-object v0, v0, Lcom/kamcord/android/KC_N$11;->a:Lcom/kamcord/android/KC_N;

    iget-object v0, v0, Lcom/kamcord/android/KC_N;->N:Landroid/widget/ImageButton;

    sget-object v1, Landroid/widget/ImageView$ScaleType;->CENTER_CROP:Landroid/widget/ImageView$ScaleType;

    invoke-virtual {v0, v1}, Landroid/widget/ImageButton;->setScaleType(Landroid/widget/ImageView$ScaleType;)V

    return-void
.end method
