.class final Lcom/kamcord/android/KC_Y$KC_a;
.super Lorg/apache/http/entity/InputStreamEntity;


# annotations
.annotation system Ldalvik/annotation/EnclosingClass;
    value = Lcom/kamcord/android/KC_Y;
.end annotation

.annotation system Ldalvik/annotation/InnerClass;
    accessFlags = 0x0
    name = "KC_a"
.end annotation


# instance fields
.field private a:Lcom/kamcord/android/KC_Y$KC_b;

.field private synthetic b:Lcom/kamcord/android/KC_Y;


# direct methods
.method public constructor <init>(Lcom/kamcord/android/KC_Y;Ljava/io/FileInputStream;J)V
    .locals 1

    iput-object p1, p0, Lcom/kamcord/android/KC_Y$KC_a;->b:Lcom/kamcord/android/KC_Y;

    invoke-direct {p0, p2, p3, p4}, Lorg/apache/http/entity/InputStreamEntity;-><init>(Ljava/io/InputStream;J)V

    return-void
.end method


# virtual methods
.method public final writeTo(Ljava/io/OutputStream;)V
    .locals 2
    .annotation system Ldalvik/annotation/Throws;
        value = {
            Ljava/io/IOException;
        }
    .end annotation

    new-instance v0, Lcom/kamcord/android/KC_Y$KC_b;

    iget-object v1, p0, Lcom/kamcord/android/KC_Y$KC_a;->b:Lcom/kamcord/android/KC_Y;

    invoke-direct {v0, v1, p1}, Lcom/kamcord/android/KC_Y$KC_b;-><init>(Lcom/kamcord/android/KC_Y;Ljava/io/OutputStream;)V

    iput-object v0, p0, Lcom/kamcord/android/KC_Y$KC_a;->a:Lcom/kamcord/android/KC_Y$KC_b;

    iget-object v0, p0, Lcom/kamcord/android/KC_Y$KC_a;->a:Lcom/kamcord/android/KC_Y$KC_b;

    invoke-super {p0, v0}, Lorg/apache/http/entity/InputStreamEntity;->writeTo(Ljava/io/OutputStream;)V

    return-void
.end method
