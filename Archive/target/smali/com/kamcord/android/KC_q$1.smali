.class final Lcom/kamcord/android/KC_q$1;
.super Ljava/lang/Object;

# interfaces
.implements Landroid/view/View$OnClickListener;


# annotations
.annotation system Ldalvik/annotation/EnclosingMethod;
    value = Lcom/kamcord/android/KC_q;-><init>(Lcom/kamcord/android/KamcordActivity;II)V
.end annotation

.annotation system Ldalvik/annotation/InnerClass;
    accessFlags = 0x0
    name = null
.end annotation


# instance fields
.field private synthetic a:La/a/a/c/KC_a;

.field private synthetic b:Lcom/kamcord/android/KC_q;


# direct methods
.method constructor <init>(Lcom/kamcord/android/KC_q;La/a/a/c/KC_a;)V
    .locals 0

    iput-object p1, p0, Lcom/kamcord/android/KC_q$1;->b:Lcom/kamcord/android/KC_q;

    iput-object p2, p0, Lcom/kamcord/android/KC_q$1;->a:La/a/a/c/KC_a;

    invoke-direct {p0}, Ljava/lang/Object;-><init>()V

    return-void
.end method


# virtual methods
.method public final onClick(Landroid/view/View;)V
    .locals 2

    iget-object v0, p0, Lcom/kamcord/android/KC_q$1;->b:Lcom/kamcord/android/KC_q;

    iget-object v1, p0, Lcom/kamcord/android/KC_q$1;->a:La/a/a/c/KC_a;

    invoke-virtual {v0, v1}, Lcom/kamcord/android/KC_q;->a(La/a/a/c/KC_a;)V

    return-void
.end method
