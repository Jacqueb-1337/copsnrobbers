.class final Lcom/kamcord/android/a/KC_h;
.super Ljava/lang/Object;


# instance fields
.field a:Ljava/lang/String;

.field b:Ljava/lang/String;

.field c:I

.field d:Z


# direct methods
.method constructor <init>(Ljava/lang/String;Ljava/lang/String;IZ)V
    .locals 1

    invoke-direct {p0}, Ljava/lang/Object;-><init>()V

    sget-object v0, Ljava/util/Locale;->ENGLISH:Ljava/util/Locale;

    invoke-virtual {p1, v0}, Ljava/lang/String;->toLowerCase(Ljava/util/Locale;)Ljava/lang/String;

    move-result-object v0

    iput-object v0, p0, Lcom/kamcord/android/a/KC_h;->a:Ljava/lang/String;

    iput-object p2, p0, Lcom/kamcord/android/a/KC_h;->b:Ljava/lang/String;

    iput p3, p0, Lcom/kamcord/android/a/KC_h;->c:I

    iput-boolean p4, p0, Lcom/kamcord/android/a/KC_h;->d:Z

    return-void
.end method
