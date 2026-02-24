.class final Lcom/kamcord/android/KC_u$KC_b;
.super Lcom/kamcord/android/KC_z;


# annotations
.annotation system Ldalvik/annotation/EnclosingClass;
    value = Lcom/kamcord/android/KC_u;
.end annotation

.annotation system Ldalvik/annotation/InnerClass;
    accessFlags = 0x0
    name = "KC_b"
.end annotation


# direct methods
.method constructor <init>(Lcom/kamcord/android/KC_u;Ljava/lang/String;Lcom/kamcord/a/a/e/KC_c;Landroid/app/Activity;)V
    .locals 0

    invoke-direct {p0, p2, p3, p4}, Lcom/kamcord/android/KC_z;-><init>(Ljava/lang/String;Lcom/kamcord/a/a/e/KC_c;Landroid/app/Activity;)V

    return-void
.end method


# virtual methods
.method public final onPageFinished(Landroid/webkit/WebView;Ljava/lang/String;)V
    .locals 0

    invoke-super {p0, p1, p2}, Lcom/kamcord/android/KC_z;->onPageFinished(Landroid/webkit/WebView;Ljava/lang/String;)V

    invoke-static {p1}, La/a/a/c/KC_a;->b(Landroid/view/ViewGroup;)V

    return-void
.end method
