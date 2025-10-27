.class final Lcom/kamcord/android/KC_m$KC_a;
.super Ljava/lang/Object;


# annotations
.annotation system Ldalvik/annotation/EnclosingClass;
    value = Lcom/kamcord/android/KC_m;
.end annotation

.annotation system Ldalvik/annotation/InnerClass;
    accessFlags = 0x8
    name = "KC_a"
.end annotation


# instance fields
.field a:Ljava/lang/String;

.field b:I

.field c:Ljava/lang/String;


# direct methods
.method constructor <init>(Ljava/lang/String;ILjava/lang/String;)V
    .locals 0

    invoke-direct {p0}, Ljava/lang/Object;-><init>()V

    iput-object p1, p0, Lcom/kamcord/android/KC_m$KC_a;->a:Ljava/lang/String;

    iput p2, p0, Lcom/kamcord/android/KC_m$KC_a;->b:I

    iput-object p3, p0, Lcom/kamcord/android/KC_m$KC_a;->c:Ljava/lang/String;

    return-void
.end method
